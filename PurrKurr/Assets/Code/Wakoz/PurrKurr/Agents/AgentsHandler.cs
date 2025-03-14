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
        private List<AgentController> _agentsPool;
        private Queue<AgentController> _agentQueue;

        private LevelsController _levelsController;

        private AgentController _currentAgent;
        private float _elapsedTime = 0f;
        private readonly float _calculationTime = 0.1f;

        public AgentsHandler()
        {
            _agentsPool = new();
            _agentQueue = new();

            _levelsController = SingleController.GetController<LevelsController>();

            AgentController.OnRegisterAgent += RegisterAgent;
            AgentController.OnDeregisterAgent += DeregisterAgent;
        }

        public void Dispose() 
        {
            AgentController.OnRegisterAgent -= RegisterAgent;
            AgentController.OnDeregisterAgent -= DeregisterAgent;
        }

        public void RegisterAgent(AgentController agent)
        {
            Debug.Log($"agent+ {agent.name}");
            _agentsPool.Add(agent);

            _agentQueue.Enqueue(agent);
        }

        public void DeregisterAgent(AgentController agent)
        {
            Debug.Log($"agent- {agent.name}");
            _agentsPool.Remove(agent);
        }

        public void UpdateProcess(float deltaTime = 0)
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > _calculationTime)
            {
                _elapsedTime -= _calculationTime;

                if (_agentQueue.Count == 0 && _agentsPool.Count() > 0)
                {
                    foreach (var agent in _agentsPool)
                    {
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
        
        private void UpdateCurrentAgent()
        {
            if (_currentAgent == null && _agentQueue.Count > 0)
            {
                _currentAgent = _agentQueue.Dequeue();
            }

            if (_currentAgent == null)
            {
                return;
            }

            var highestPriorityGoal = GetHighestPriorityGoal(_currentAgent);
            if (highestPriorityGoal != null)
            {
                GenerateAndExecutePlan(_currentAgent, highestPriorityGoal);
            }

            _currentAgent = null;
        }

        private void GenerateAndExecutePlan(AgentController agent, GoalData goalData)
        {
            var plan = CreatePlan(agent, goalData);

            ExecutePlan(agent, plan, goalData.Goal);
        }

        private (float xMove, float yForce) CreatePlan(AgentController agent, GoalData goal)
        {
            var character = agent.CharacterController;

            if (character == null || character.GetHpPercent() == 0)
                return (0, 0);

            var characterPosition = character.GetCenterPosition();

            switch (goal.Goal)
            {
                case Definitions.AgentGoal.Explore:

                var consumables = GetClosestTargets(goal.TargetStrings, characterPosition);
                if (consumables.Count == 0)
                {
                    return (character.Stats.WalkSpeed * character.State.GetFacingRightAsInt(), 0);
                }
                var exploreSpeed = character.Stats.WalkSpeed;
                var closestConsumable = consumables.FirstOrDefault();
                var dirToNearbyObject = (closestConsumable.position - characterPosition);
                var distanceToConsumbeable = Vector2.Distance(characterPosition, closestConsumable.position);
                var senses = character.Senses?.GetComponent<CircleCollider2D>();
                if (senses != null && distanceToConsumbeable < senses.radius)
                {
                    exploreSpeed = character.Stats.RunSpeed;
                    if (dirToNearbyObject.y > 0 && character.State.IsTouchingAnySurface())
                    {
                        return (character.Stats.JumpForce*.25f * dirToNearbyObject.normalized.x, character.Stats.JumpForce*.75f);
                    }
                }
                if (character.State.IsFrontWall() && character.GetVelocity().magnitude < 3)
                {
                    exploreSpeed *= -1;
                    character.FlipCharacterTowardsPoint(-exploreSpeed > 0);
                }
                var hasReachedZone = dirToNearbyObject.magnitude < 2; // 2 is the minimum distance to the consumeable
                if (hasReachedZone)
                {
                    Debug.Log($"{agent.name} consuming {closestConsumable.name}");
                    var consumeable = closestConsumable.GetComponent<ConsumeableItemController>();
                    if (consumeable != null)
                    {
                        consumeable.Consume(1);
                    }
                }
                return (-exploreSpeed * dirToNearbyObject.normalized.x, 0);

                case Definitions.AgentGoal.Protect:
                var protectee = GetClosestTargets(goal.TargetStrings, characterPosition);
                if (protectee.Count == 0)
                {
                    return (0, 0);
                }
                var closestProtectee = protectee.FirstOrDefault();
                var dirToClosestProtecee = (closestProtectee.position - characterPosition);
                return (-character.Stats.RunSpeed * dirToClosestProtecee.normalized.x, 0);

                case Definitions.AgentGoal.Fight:

                var nearbyCharacters = GetNearbyAttackers(agent);
                var foe = nearbyCharacters?.FirstOrDefault();
                if (foe == null)
                {
                    return (0, 0);
                }
                var dirToFoe = character.GetCenterPosition() - foe.GetCenterPosition();
                var hasReachedFoe = dirToFoe.magnitude < 5; // 2 is the minimum distance to attack
                if (hasReachedFoe && character.State.CanPerformAction() && Random.Range(0, 100) < 4)
                {
                    Debug.Log($"{agent.name} attacking {foe.name}");
                    SingleController.GetController<GameplayController>().CombatLogic(character, Definitions.ActionType.Attack, foe.GetCenterPosition(), character.NearbyInteractables());
                }
                return (dirToFoe.normalized.x * character.Stats.RunSpeed, 0);

                case Definitions.AgentGoal.Run:

                var nearbyInteractables = GetNearbyAttackers(agent);
                var attacker = nearbyInteractables?.FirstOrDefault();
                if (attacker == null)
                {
                    return (character.State.GetFacingRightAsInt() * character.Stats.RunSpeed, 0);
                }
                return ((attacker.GetCenterPosition() - character.GetCenterPosition()).normalized.x * character.Stats.SprintSpeed, 0);
            }

            return (0, 0);
        }

        private void ExecutePlan(AgentController agent, (float xMove, float yForce) plan, Definitions.AgentGoal goal)
        {
            // Set goal for display in the editor and inspector
            agent.CurrentGoal(goal);

            // Execute the plan on the agent's controller
            agent.CharacterController.DoMove(plan.xMove);
            if (plan.yForce > 0)
            {
                agent.CharacterController.SetForceDir(new Vector2(plan.xMove, plan.yForce));
            }
        }

        private List<Transform> GetClosestTargets(List<string> targets, Vector3 point)
        {
            var res = new List<Transform>();

            res.AddRange(_levelsController.GetTaggedObject(targets));

            return res.OrderBy(obj => Vector2.Distance(obj.transform.position, point)).ToList();
        }

        private GoalData GetHighestPriorityGoal(AgentController agent)
        {
            var goals = agent.Goals;

            foreach (var goal in goals)
                if (IsValidGoal(goal, agent))
                    return goal;

            return goals.FirstOrDefault();
        }

        private bool IsValidGoal(GoalData goal, AgentController agent)
        {
            var conditions = goal.Conditions;
            foreach (var condition in conditions)
            {
                switch (condition.Condition)
                {
                    case Definitions.GoalCondition.HpIsAboveRange01:
                    if (agent.CharacterController.GetHpPercent() < condition.targetValue)
                        return false;
                    break;

                    case Definitions.GoalCondition.HasNearbyAttackers:
                    var nearbyInteractables = GetNearbyAttackers(agent);
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

        private List<Character2DController> GetNearbyCharacters(AgentController agent)
        {
            var nearby = agent.CharacterController.NearbyInteractables();
            var nearbyInteractables = new List<Character2DController>();
            foreach (var item in nearby)
            {
                var interactable = item.GetComponent<IInteractable>();
                if (interactable == null)
                    continue;

                var interactableBody = interactable.GetInteractableBody() as Character2DController;
                if (interactableBody == null)
                    continue;

                nearbyInteractables.Add(interactableBody);
            }
            return nearbyInteractables;
        }

        private List<Character2DController> GetNearbyAttackers(AgentController agent)
        {
            var nearby = agent.CharacterController.State.GetLatestInteraction();
            var res = new List<Character2DController>();

            var interactableBody = nearby as Character2DController;
            if (interactableBody == null)
                return res;

            var senses = interactableBody.Senses?.GetComponent<CircleCollider2D>();
            var dis = Vector2.Distance(interactableBody.GetCenterPosition(), agent.transform.position);
            if (dis > senses.radius)
                return res;

            res.Add(interactableBody);

            return res;
        }

        private List<Transform> GetNearbyConsumeables(AgentController agent, GoalData goal)
        {
            List<Transform> res = new();
            var consumables = GetClosestTargets(goal.TargetStrings, agent.CharacterController.GetCenterPosition());
            if (consumables.Count > 0)
            {
                foreach (var item in consumables)
                {
                    var consumeableItem = item.GetComponent<ConsumeableItemController>();
                    if (consumeableItem == null || consumeableItem.GetData().quantity == 0)
                    {
                        continue;
                    }
                    res.Add(item);
                }
            }
            return res;
        }

    }
}