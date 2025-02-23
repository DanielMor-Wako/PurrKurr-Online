using UnityEngine;

namespace Code.Wakoz.Utils.BitwiseUtils
{
    public static class BitwiseUtility
    {
        #region INT METHODS

        private const int BitsPerInt = 32;

        /// <summary>
        /// Internal function to handle validation and returning the arrayIndex and bitPosition
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arrayIndex"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        private static bool GetIntIndex(int index, out int arrayIndex, out int bitPosition)
        {
            if (index < 0 || index >= BitsPerInt)
            {
                Debug.LogError("Index out of range for bitmaskArray.");
                arrayIndex = bitPosition = 0;
                return false;
            }

            arrayIndex = index / BitsPerInt;
            bitPosition = index % BitsPerInt;
            return true;
        }

        /// <summary>
        /// Sets a bit at the specified index to 1 (collected).
        /// </summary>
        public static void SetBit(this int[] bitmaskArray, int index)
        {
            int arrayIndex, bitPosition;
            if (!GetIntIndex(index, out arrayIndex, out bitPosition))
                return;

            bitmaskArray[arrayIndex] |= (1 << bitPosition);
        }

        /// <summary>
        /// Clears a bit at the specified index to 0 (not collected).
        /// </summary>
        public static void ClearBit(this int[] bitmaskArray, int index)
        {
            int arrayIndex, bitPosition;
            if (!GetIntIndex(index, out arrayIndex, out bitPosition))
                return;

            bitmaskArray[arrayIndex] &= ~(1 << bitPosition);
        }

        /// <summary>
        /// Checks if a bit at the specified index is set (collected).
        /// </summary>
        public static bool IsBitSet(this int[] bitmaskArray, int index)
        {
            int arrayIndex, bitPosition;
            if (!GetIntIndex(index, out arrayIndex, out bitPosition))
                return false;

            return (bitmaskArray[arrayIndex] & (1 << bitPosition)) != 0;
        }

        /// <summary>
        /// Sets all bits in the bitmask array to 1.
        /// </summary>
        public static void SetAllBits(this int[] bitmaskArray)
        {
            for (int i = 0; i < bitmaskArray.Length; i++)
            {
                bitmaskArray[i] = ~0; // Set all 32 bits to 1
            }
        }

        /// <summary>
        /// Sets only specific bits in the bitmask array to 1.
        /// </summary>
        public static void SetAllBits(this int[] bitmaskArray, int totalBits)
        {
            int fullInts = totalBits / BitsPerInt;
            int remainingBits = totalBits % BitsPerInt;

            for (int i = 0; i < fullInts; i++)
            {
                bitmaskArray[i] = ~0; // Set all 32 bits to 1
            }

            if (remainingBits > 0)
            {
                bitmaskArray[fullInts] = (1 << remainingBits) - 1; // Set only the remaining bits to 1
            }
        }

        /// <summary>
        /// Clears all bits in the bitmask array to 0.
        /// </summary>
        public static void ClearAllBits(this int[] bitmaskArray)
        {
            for (int i = 0; i < bitmaskArray.Length; i++)
            {
                bitmaskArray[i] = 0;
            }
        }

        /// <summary>
        /// Counts the total number of active bits (collected items) in the bitmask array.
        /// </summary>
        public static int CountActiveBits(this int[] bitmaskArray)
        {
            int count = 0;

            foreach (int bitmask in bitmaskArray)
            {
                count += CountBits(bitmask);
            }

            return count;
        }

        /// <summary>
        /// Counts the number of active bits (1s) in an integer using a bitwise population count algorithm.
        /// Brian Kernighan’s Algorithm to count the number of 1s in an integer
        /// n &= (n - 1); this line clears the least significant bit set to 1
        /// Repeat this operation until n becomes 0, Counts the iterations which equals the number of 1s in the integer
        /// </summary>
        private static int CountBits(int n)
        {
            int count = 0;

            while (n != 0)
            {
                n &= (n - 1); // Clears the least significant bit set to 1
                count++;
            }

            return count;
        }
        #endregion

        #region ULONG[] METHODS

        private const int BitsPerUlong = 64;

        /// <summary>
        /// Internal function to handle validation and returning the arrayIndex and bitPosition
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arrayIndex"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        private static bool GetUlongIndex(int index, out int arrayIndex, out int bitPosition)
        {
            if (index < 0 || index >= BitsPerUlong)
            {
                Debug.LogError("Index out of range for bitmaskArray.");
                arrayIndex = bitPosition = 0;
                return false;
            }

            arrayIndex = index / BitsPerUlong;
            bitPosition = index % BitsPerUlong;
            return true;
        }

        /// <summary>
        /// Sets a bit at the specified index to 1 (collected).
        /// </summary>
        public static void SetBit(this ulong[] bitmaskArray, int index)
        {
            int arrayIndex, bitPosition;
            if (!GetUlongIndex(index, out arrayIndex, out bitPosition))
                return;

            bitmaskArray[arrayIndex] |= (1UL << bitPosition);
        }

        /// <summary>
        /// Clears a bit at the specified index to 0 (not collected).
        /// </summary>
        public static void ClearBit(this ulong[] bitmaskArray, int index)
        {
            int arrayIndex, bitPosition;
            if (!GetUlongIndex(index, out arrayIndex, out bitPosition))
                return;

            bitmaskArray[arrayIndex] &= ~(1UL << bitPosition);
        }

        /// <summary>
        /// Checks if a bit at the specified index is set (collected).
        /// </summary>
        public static bool IsBitSet(this ulong[] bitmaskArray, int index)
        {
            int arrayIndex, bitPosition;
            if (!GetUlongIndex(index, out arrayIndex, out bitPosition))
                return false;

            return (bitmaskArray[arrayIndex] & (1UL << bitPosition)) != 0;
        }

        /// <summary>
        /// Sets all bits in the bitmask array to 1.
        /// </summary>
        public static void SetAllBits(this ulong[] bitmaskArray)
        {
            for (int i = 0; i < bitmaskArray.Length; i++)
            {
                bitmaskArray[i] = ~0UL; // Set all 64 bits to 1
            }
        }

        /// <summary>
        /// Sets only specific bits in the bitmask array to 1.
        /// </summary>
        public static void SetAllBits(this ulong[] bitmaskArray, int totalBits)
        {
            int fullUlongs = totalBits / BitsPerUlong;
            int remainingBits = totalBits % BitsPerUlong;

            for (int i = 0; i < fullUlongs; i++)
            {
                bitmaskArray[i] = ~0UL; // Set all 64 bits to 1
            }

            if (remainingBits > 0)
            {
                bitmaskArray[fullUlongs] = (1UL << remainingBits) - 1; // Set only the remaining bits to 1
            }
        }

        /// <summary>
        /// Clears all bits in the bitmask array to 0.
        /// </summary>
        public static void ClearAllBits(this ulong[] bitmaskArray)
        {
            for (int i = 0; i < bitmaskArray.Length; i++)
            {
                bitmaskArray[i] = 0UL;
            }
        }

        /// <summary>
        /// Counts the total number of active bits (collected items) in the bitmask array.
        /// </summary>
        public static int CountActiveBits(this ulong[] bitmaskArray)
        {
            int count = 0;

            foreach (ulong bitmask in bitmaskArray)
            {
                count += CountBits(bitmask);
            }

            return count;
        }

        /// <summary>
        /// Counts the number of active bits (1s) in a ulong using a bitwise population count algorithm.
        /// Brian Kernighan’s Algorithm to count the number of 1s in a ulong.
        /// </summary>
        private static int CountBits(ulong n)
        {
            int count = 0;

            while (n != 0)
            {
                n &= (n - 1);
                count++;
            }

            return count;
        }
        #endregion
    }
}