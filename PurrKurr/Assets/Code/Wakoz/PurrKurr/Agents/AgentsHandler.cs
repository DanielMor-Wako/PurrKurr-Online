using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.ConsumeableItems;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.PurrKurr.Screens.Levels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Agents
{
    public class AgentsHandler : IUpdateProcessHandler
    {
        private const int AttackIndexMin = (int)Definitions.ActionType.Attack;
        private const int AttackIndexMax = (int)Definitions.ActionType.Grab + 1;
        private List<AgentController> _agentsPool;
        private Queue<AgentController> _agentQueue;

        private LevelsController _levelsController;

        private AgentController _currentAgent;
        private float _elapsedTime = 0f;
        private readonly float _calculationRate = 0.1f;

        public AgentsHandler() {
            _agentsPool = new();
            _agentQueue = new();

            _levelsController = SingleController.GetController<LevelsController>();

            AgentController.OnRegisterAgent += RegisterAgent;
            AgentController.OnDeregisterAgent += DeregisterAgent;
        }

        public void Dispose() {

            AgentController.OnRegisterAgent -= RegisterAgent;
            AgentController.OnDeregisterAgent -= DeregisterAgent;
        }

        public void RegisterAgent(AgentController agent) {
            Debug.Log($"agent+ {agent.name}");
            _agentsPool.Add(agent);

            _agentQueue.Enqueue(agent);
        }

        public void DeregisterAgent(AgentController agent) {
            Debug.Log($"agent- {agent.name}");
            _agentsPool.Remove(agent);

            _agentQueue = new Queue<AgentController>(_agentQueue.Where(o => !o.Equals(agent)));
        }

        public void UpdateProcess(float deltaTime = 0) {

            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > _calculationRate) {
                _elapsedTime -= _calculationRate;

                if (_agentQueue.Count == 0 && _agentsPool.Count() > 0) {
                    foreach (var agent in _agentsPool) {
                        if (agent == null)
                            continue;

                        var characterAgent = agent.CharacterController;
                        if (characterAgent != null && characterAgent.GetHpPercent() == 0 && characterAgent.GetVelocity().magnitude < 0.01f)
                            continue;

                        _agentQueue.Enqueue(agent);
                    }
                }
            }

            UpdateCurrentAgent();
        }

        private void UpdateCurrentAgent() {

            if (_currentAgent == null && _agentQueue.Count > 0) {
                _currentAgent = _agentQueue.Dequeue();
            }

            if (_currentAgent == null) {
                return;
            }

            var highestPriorityGoal = GetHighestPriorityGoal(_currentAgent);
            if (highestPriorityGoal != null) {
                GenerateAndExecutePlan(_currentAgent, highestPriorityGoal);
            }

            _currentAgent = null;
        }

        private void GenerateAndExecutePlan(AgentController agent, GoalData goalData) {

            var targetsTransform = GetNearbyTargetTransform(agent, goalData);

            var plan = CreatePlan(agent, goalData, targetsTransform);

            ExecutePlan(agent, plan, goalData.Goal);

            CheckCloseByToTargetsTransform(agent, goalData, targetsTransform);
        }

        private Transform GetNearbyTargetTransform(AgentController agent, GoalData goal) {

            var character = agent.CharacterController;

            if (character == null)
                return agent.transform;

            var newTargetTransform = character.transform;

            if (character.GetHpPercent() == 0)
                return newTargetTransform;

            Transform returnTransforms = null;

            switch (goal.Goal) {
                case Definitions.AgentGoal.Explore:

                    var consumables =
                    GetTargetsAndOrderByClosestDistance(goal.TargetStrings, newTargetTransform.position)
                    .Where(o => o.GetComponent<ConsumeableItemController>()?.GetData().quantity > 0).ToArray();

                    if (consumables.Length == 0) {
                        return null;
                    }
                    returnTransforms = consumables.FirstOrDefault();
                    break;

                case Definitions.AgentGoal.Protect:

                    var protectee = GetTargetsAndOrderByClosestDistance(goal.TargetStrings, newTargetTransform.position);
                    if (protectee.Count == 0) {
                        return null;
                    }
                    returnTransforms = protectee.FirstOrDefault();
                    break;

                case Definitions.AgentGoal.Fight:

                    var nearbyCharacters = GetLatestAttacker(agent);
                    var foe = nearbyCharacters?.FirstOrDefault();
                    if (foe == null) {
                        return null;
                    }
                    returnTransforms = foe.transform;
                    break;

                case Definitions.AgentGoal.Run:

                    var nearbyInteractables = GetLatestAttacker(agent);
                    var attacker = nearbyInteractables?.FirstOrDefault();
                    if (attacker == null) {
                        return null;
                    }
                    returnTransforms = attacker.transform;
                    break;
            }

            return returnTransforms;
        }

        private (float xMove, float yForce) CreatePlan(AgentController agent, GoalData goal, Transform targetsTransform) {

            var character = agent.CharacterController;

            if (character == null || character.GetHpPercent() == 0 || targetsTransform == null ||
                !character.State.CanPerformContinousRunning() || character.State.IsInterraptibleAnimation() || character.State.IsStunnedState())
                return (0, 0);

            var characterPosition = character.GetCenterPosition();

            switch (goal.Goal) {
                case Definitions.AgentGoal.Explore:

                    var exploreSpeed = character.Stats.WalkSpeed;
                    var closestConsumable = targetsTransform.position;
                    var dirToNearbyObject = (closestConsumable - characterPosition);
                    var distanceToConsumbeable = Vector2.Distance(characterPosition, closestConsumable);
                    if (distanceToConsumbeable < character.SensesRadius()) {
                        exploreSpeed = character.Stats.RunSpeed;
                        if (dirToNearbyObject.y > 0 && character.State.IsTouchingAnySurface()) {
                            return (character.Stats.JumpForce * .25f * dirToNearbyObject.normalized.x, character.Stats.JumpForce * .75f);
                        }
                    }
                    if (character.State.IsFrontWall() && character.GetVelocity().magnitude < 3) {
                        exploreSpeed *= -1;
                        character.FlipCharacterTowardsPoint(-exploreSpeed > 0);
                    }
                    return (-exploreSpeed * dirToNearbyObject.normalized.x, 0);

                case Definitions.AgentGoal.Protect:
                    var closestProtectee = targetsTransform.position;
                    var dirToClosestProtecee = (closestProtectee - characterPosition);
                    return (-character.Stats.RunSpeed * dirToClosestProtecee.normalized.x, 0);

                case Definitions.AgentGoal.Fight:

                    var foe = targetsTransform.GetComponent<IInteractableBody>();
                    if (foe == null) {
                        return (0, 0);
                    }
                    var dirToFoe = foe.GetCenterPosition() - character.GetCenterPosition();
                    var isAgentFightingAgent = targetsTransform.GetComponent<AgentController>() != null;
                    var isAttacking = isAgentFightingAgent || IsValidDedicatedAttackerAgainstPlayer(agent);
                    var distanceFromFoe = isAttacking ? 2 : 5;
                    var positionInfrontOfFoe = foe.GetCenterPosition() - dirToFoe.normalized * distanceFromFoe;// * -(character.LegsRadius);
                    var dirToPositionInfrontOfFoe = positionInfrontOfFoe - character.GetCenterPosition();
                    return (-character.Stats.WalkSpeed * dirToPositionInfrontOfFoe.x, 0);

                case Definitions.AgentGoal.Run:

                    var attacker = targetsTransform;
                    if (attacker == null) {
                        return (character.State.GetFacingRightAsInt() * character.Stats.RunSpeed, 0);
                    }
                    return ((attacker.position - character.GetCenterPosition()).normalized.x * character.Stats.SprintSpeed, 0);
            }

            return (0, 0);
        }

        private bool CheckCloseByToTargetsTransform(AgentController agent, GoalData goal, Transform targetsTransform) {

            var character = agent.CharacterController;

            if (character == null || character.GetHpPercent() == 0 || targetsTransform == null)
                return false;

            var characterPosition = character.GetCenterPosition();

            var dirToNearbyObject = characterPosition - targetsTransform.position;
            var isNearTargetsTransform = dirToNearbyObject.magnitude < character.SensesRadius();
            if (!isNearTargetsTransform) {
                return false;
            }

            switch (goal.Goal) {
                case Definitions.AgentGoal.Explore:

                    var consumeable = targetsTransform.GetComponent<ConsumeableItemController>();
                    if (consumeable != null && dirToNearbyObject.magnitude < 2) {
                        consumeable.Consume(1);
                        return true;
                    }
                    break;

                case Definitions.AgentGoal.Fight:

                    var isAgentFightingAgent = targetsTransform.GetComponent<AgentController>() != null;
                    if (dirToNearbyObject.magnitude < 3 && character.State.CanPerformAction()
                        && (isAgentFightingAgent && Random.Range(0, 100) < 2) || IsValidDedicatedAttackerAgainstPlayer(agent)) {
                        if (!isAgentFightingAgent) { _agentDedicatedToAttackMainHero = null; }
                        var foe = targetsTransform.GetComponent<IInteractableBody>();
                        Debug.Log($"{agent.name} attacking {foe.GetTransform().name}");
                        int randomCombatAbility = Random.Range(AttackIndexMin, AttackIndexMax);
                        /*if (randomCombatAbility == (int)Definitions.ActionType.Block) {
                            randomCombatAbility = (int)Definitions.ActionType.Grab;
                        }*/
                        _characterActionExecuter ??= SingleController.GetController<GameplayController>().CharacterActionExecuter;
                        _characterActionExecuter.OnActionStarted(character, new Screens.Ui_Controller.ActionInput((Definitions.ActionType)randomCombatAbility, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, Vector2.zero));
                        //SingleController.GetController<GameplayController>().CombatLogic(character, (Definitions.ActionType)randomCombatAbility);
                        return true;
                    }
                    break;

                case Definitions.AgentGoal.Protect:

                    break;

            }

            return false;
        }

        private AgentController _agentDedicatedToAttackMainHero;
        private bool IsValidDedicatedAttackerAgainstPlayer(AgentController agent) {

            var isValid = IsCombatInteractionValid();

            if (isValid) {
                _agentDedicatedToAttackMainHero = agent;
                _recentAttackInteraction = Time.realtimeSinceStartup;
            }

            return agent == _agentDedicatedToAttackMainHero;
        }

        // todo: move this to gameBalanceManager script
        private float _recentAttackInteraction = 0;
        private CharacterActionExecuter _characterActionExecuter;
        private readonly float CombatInteractionRateInSeconds = 2.0f;
        private bool IsCombatInteractionValid() {
            var elapsedTime = Time.realtimeSinceStartup - _recentAttackInteraction;
            var percent = elapsedTime / CombatInteractionRateInSeconds;
            var isAttacking = percent > 1;
            if (isAttacking) {
                _recentAttackInteraction = Time.realtimeSinceStartup;
            }
            return isAttacking;
        }

        private void ExecutePlan(AgentController agent, (float xMove, float yForce) plan, Definitions.AgentGoal goal) {
            // Set goal for display in the editor and inspector
            agent.SetCurrentGoal(goal);

            // Execute the plan on the agent's controller
            agent.CharacterController.DoMove(plan.xMove);
            if (plan.yForce > 0) {
                agent.CharacterController.SetForceDir(new Vector2(plan.xMove, plan.yForce));
            }
            /*_characterActionExecuter ??= SingleController.GetController<GameplayController>().CharacterActionExecuter;
            if (plan.xMove == 0) {
                _characterActionExecuter.OnNavigationEnded(agent.CharacterController, new Screens.Ui_Controller.ActionInput(Definitions.ActionType.Movement, Definitions.ActionTypeGroup.Navigation, Vector2.zero, Time.time, new Vector2(plan.xMove, plan.yForce)));
            } else {
                _characterActionExecuter.OnNavigationOngoing(agent.CharacterController, new Screens.Ui_Controller.ActionInput(Definitions.ActionType.Movement, Definitions.ActionTypeGroup.Navigation, Vector2.zero, Time.time, new Vector2(plan.xMove, plan.yForce)));
            }

            if (plan.yForce > 0) {
                _characterActionExecuter.OnActionStarted(agent.CharacterController, new Screens.Ui_Controller.ActionInput(Definitions.ActionType.Jump, Definitions.ActionTypeGroup.Action, new Vector2(plan.xMove, plan.yForce), Time.time, Vector2.zero));
            }*/
        }
    
        private List<Transform> GetTargetsAndOrderByClosestDistance(List<string> targets, Vector3 point) {

            var res = new List<Transform>();
            res.AddRange(_levelsController.GetTaggedObject(targets));
            return res.OrderBy(obj => Vector2.Distance(obj.transform.position, point)).ToList();
        }

        private GoalData GetHighestPriorityGoal(AgentController agent) {

            var goals = agent.Goals;

            foreach (var goal in goals)
                if (ValidateGoal(goal, agent))
                    return goal;

            return goals.FirstOrDefault();
        }

        private bool ValidateGoal(GoalData goal, AgentController agent) {

            var conditions = goal.Conditions;
            foreach (var condition in conditions) {
                switch (condition.Condition) {
                    case Definitions.GoalCondition.IsHpPercentAboveRange01:
                        if (agent.CharacterController.GetHpPercent() < condition.targetValue)
                            return false;
                        break;

                    case Definitions.GoalCondition.HasNearbyAttackers:
                        var nearbyInteractables = GetLatestAttacker(agent);
                        if (condition.targetValue == 0 && nearbyInteractables.Count > 0)
                            return false;

                        var hasMinimumAttackers = nearbyInteractables.Count >= condition.targetValue;
                        if (!hasMinimumAttackers)
                            return false;
                        break;

                    case Definitions.GoalCondition.HasNearbyConsumeables:
                        var nearbyConsumeables = GetNearbyConsumeables(agent, goal);
                        if (nearbyConsumeables.Count < condition.targetValue)
                            return false;
                        break;
                }
            }

            return true;
        }

        // todo: Move to shared interactableBody class as extention for character and objects in the game
        #region CharacterExtentions
        private List<Character2DController> GetNearbyCharacters(AgentController agent) {

            var nearby = agent.CharacterController.NearbyInteractables();
            var nearbyInteractables = new List<Character2DController>();
            foreach (var item in nearby) {
                var interactable = item.GetComponent<IInteractable>();
                if (interactable == null)
                    continue;

                var interactableBody = interactable.GetInteractableBody() as Character2DController;
                if (interactableBody == null || interactableBody.GetHpPercent() == 0)
                    continue;

                nearbyInteractables.Add(interactableBody);
            }

            return nearbyInteractables;
        }

        private List<Character2DController> GetLatestAttacker(AgentController agent) {

            var nearby = agent.CharacterController.State.GetLatestInteraction();
            var res = new List<Character2DController>();

            var interactableBody = nearby as Character2DController;
            if (interactableBody != null) {

                var dis = Vector2.Distance(interactableBody.GetCenterPosition(), agent.transform.position);
                if (dis > interactableBody.SensesRadius())
                    return res;

                res.Add(interactableBody);
            }

            return res;
        }

        private List<Transform> GetNearbyConsumeables(AgentController agent, GoalData goal) {

            List<Transform> res = new();
            var consumables = GetTargetsAndOrderByClosestDistance(goal.TargetStrings, agent.CharacterController.GetCenterPosition());
            if (consumables.Count > 0) {
                foreach (var item in consumables) {
                    var consumeableItem = item.GetComponent<ConsumeableItemController>();
                    if (consumeableItem == null || consumeableItem.GetData().quantity == 0) {
                        continue;
                    }
                    res.Add(item);
                }
            }
            return res;
        }
    }

    #endregion
}