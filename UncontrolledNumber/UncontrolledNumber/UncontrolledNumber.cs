/**
 * By Samuel Sticklen 2023
 * 
 * Code that makes use of the BigInteger class to provide unsized decimal
 * operations.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace System.Numerics
{
    /// <summary>
    /// Represents a number with no max/min or precisional boundaries
    /// (decimal precision set via DivisionAccuracy)
    /// </summary>
    public struct UncontrolledNumber
        : IComparable
    {
        #region Properties & Fields

        /// <summary>
        /// Gets the complete integer, ignoring decimal boundaries.
        /// e.g. 1.3 = 13
        /// </summary>
        public BigInteger CompleteInteger
        {
            get
            {
                if (IsNegative)
                    return -BigInteger.Pow(10, DecimalDigits + DecimalZeroes) * -Integer - Decimal;
                return BigInteger.Pow(10, DecimalDigits + DecimalZeroes) * Integer + Decimal;
            }
        }

        /// <summary>
        /// Represents the integer part of the number (i in i.000)
        /// </summary>
        public BigInteger Integer { get; private set; }

        /// <summary>
        /// Represents the decimal part of the number (d in 0.0dd),
        /// with exclusion to leading decimal zeroes.
        /// </summary>
        public BigInteger Decimal { get; private set; }

        /// <summary>
        /// If true, then this number is infinity.
        /// </summary>
        public bool IsInfinity { get; private set; } = false;

        /// <summary>
        /// If true, then this number is exactly zero.
        /// </summary>
        public bool IsZero
        {
            get
            {
                return !IsInfinity && Integer == 0 && Decimal == 0;
            }
        }

        /// <summary>
        /// Gets an instance of an infinity number.
        /// </summary>
        public static UncontrolledNumber Infinity
        {
            get
            {
                return new UncontrolledNumber() { IsInfinity = true };
            }
        }

        /// <summary>
        /// Gets how many digits are in the decimal
        /// </summary>
        public int DecimalDigits { get; private set; }

        /// <summary>
        /// Gets how many zeroes lead the first decimal digit.
        /// </summary>
        public int DecimalZeroes { get; private set; }

        /// <summary>
        /// Returns true if the number is negative
        /// </summary>
        public bool IsNegative
        {
            get
            {
                if (Integer == 0 && Decimal == 0) return false;
                return Integer.Sign == -1 || ForceNegative;
            }
        }

        /// <summary>
        /// When the integer is zero, the number may forced to be negative.
        /// </summary>
        private bool ForceNegative = false;

        #endregion
        #region Methods
        #region Constructors
        public UncontrolledNumber(double val)
        {
            Integer = (long)val;
            if (val < 0)
                ForceNegative = true;
            Decimal = 0;
            DecimalDigits = 0;
            DecimalZeroes = 0;
            string _decimal = val.ToString("0.000000000000000");
            int decIndex = _decimal.IndexOf('.');
            if (decIndex != -1)
            {
                string decimalPart = _decimal.Substring(decIndex + 1);
                (Decimal, DecimalDigits) = ConvertToDecimal(BigInteger.Parse(decimalPart));
                for (int i = 0; i < decimalPart.Length; i++)
                {
                    if (decimalPart[i] != '0')
                        break;
                    DecimalZeroes++;
                }
                if (Decimal == 0)
                {
                    DecimalDigits = 0;
                    DecimalZeroes = 0;
                }
            }
        }
        public UncontrolledNumber(BigInteger integer, BigInteger _decimal, int zeroes, bool negative)
        {
            if (zeroes < 0) throw new ArgumentOutOfRangeException("zeroes");
            Integer = integer;
            (Decimal, DecimalDigits) = ConvertToDecimal(_decimal);
            DecimalZeroes = zeroes;
            ForceNegative = negative;
        }
        #endregion
        /// <summary>
        /// Converts the integer into a shortened integer (removes negative and unnecessary zeroes).
        /// </summary>
        private static (BigInteger, int) ConvertToDecimal(BigInteger integer)
        {
            if (integer < 0) integer = -integer; // Remove negative.
            while (integer % 10 == 0 && integer != 0) integer /= 10;

            return (integer, integer.ToString().Length);
        }
        private static BigInteger ReversePositiveInteger(BigInteger integer)
        {
            BigInteger newInteger = new BigInteger();
            while (integer > 0)
            {
                newInteger *= 10;
                newInteger += integer % 10;
                integer /= 10;
            }
            return newInteger;
        }

        /// <summary>
        /// Gets the length of the positive integer.
        /// </summary>
        private static int GetDigitCount(BigInteger integer)
        {
            return integer.ToString().Length;
        }

        /// <summary>
        /// Compares two decimal parts of two uncontrolled numbers
        /// </summary>
        /// <param name="a">number 1</param>
        /// <param name="b">number 2</param>
        /// <returns>if equal, then 0, else if frac a is smaller than b then -1 else 1</returns>
        private static int CompareDecimal(UncontrolledNumber a, UncontrolledNumber b)
        {
            if (a == b) return 0;

            // Check zeroes-case (0.001 a < 0.1 b) 
            if (a.DecimalZeroes > b.DecimalZeroes) return -1;
            else if (a.DecimalZeroes < b.DecimalZeroes) return 1;

            (BigInteger aFrac, BigInteger bFrac) = MatchDecimalDigits(a, b);
            if (aFrac < bFrac) return 1;
            return -1;
        }

        /// <summary>
        /// Extracts the _decimals from two numbers with an equal number of digits.
        /// </summary>
        private static (BigInteger, BigInteger) MatchDecimalDigits(UncontrolledNumber a, UncontrolledNumber b)
        {
            BigInteger aFrac = a.Decimal;
            int aFracDigits = a.DecimalDigits;

            BigInteger bFrac = b.Decimal;
            int bFracDigits = b.DecimalDigits;

            // Match number of digits
            while (aFracDigits < bFracDigits)
            {
                aFrac *= 10;
                aFracDigits++;
            }
            while (bFracDigits < aFracDigits)
            {
                bFrac *= 10;
                bFracDigits++;
            }

            // Figure out order (zeroes) so that order remains the same.
            int orderDifference = a.DecimalZeroes - b.DecimalZeroes;
            if (orderDifference > 0) // b is of the lower order
            {
                aFrac *= BigInteger.Pow(10, orderDifference);
            }
            else if (orderDifference < 0) // a is of the lower order
            {
                bFrac *= BigInteger.Pow(10, -orderDifference);
            }

            return (aFrac, bFrac);
        }

        /// <summary>
        /// Extracts the _decimals from two numbers with an equal number of digits, and
        /// returns the complete integers if the numbers had the resulting decimal values.
        /// Only returns a positive value.
        /// </summary>
        private static (BigInteger, BigInteger) GetMatchedCompleteInteger(UncontrolledNumber a, UncontrolledNumber b)
        {

            BigInteger aFrac = a.Decimal;
            int aFracDigits = a.DecimalDigits;

            BigInteger bFrac = b.Decimal;
            int bFracDigits = b.DecimalDigits;

            // Match number of digits
            while (aFracDigits < bFracDigits)
            {
                aFrac *= 10;
                aFracDigits++;
            }
            while (bFracDigits < aFracDigits)
            {
                bFrac *= 10;
                bFracDigits++;
            }

            // Figure out order (zeroes) so that order remains the same.
            int orderDifference = a.DecimalZeroes - b.DecimalZeroes;
            if (orderDifference > 0) // b is of the lower order
            {
                aFrac *= BigInteger.Pow(10, orderDifference);
            }
            else if (orderDifference < 0) // a is of the lower order
            {
                bFrac *= BigInteger.Pow(10, -orderDifference);
            }

            BigInteger aa = a.IsNegative ? -a.Integer : a.Integer;
            BigInteger bb = b.IsNegative ? -b.Integer : b.Integer;

            return (BigInteger.Pow(10, aFracDigits) * aa + aFrac, BigInteger.Pow(10, bFracDigits) * bb + bFrac);
        }

        #region Operators
        public static implicit operator UncontrolledNumber(double num) => new UncontrolledNumber(num);
        public static implicit operator UncontrolledNumber(long num) => new UncontrolledNumber(num, 0, 0, false);
        public static implicit operator UncontrolledNumber(int num) => new UncontrolledNumber(num, 0, 0, false);

        public static UncontrolledNumber operator +(UncontrolledNumber a) => a;
        public static UncontrolledNumber operator -(UncontrolledNumber a) => new UncontrolledNumber(-a.Integer, a.Decimal, a.DecimalZeroes, !a.IsNegative);

        public static UncontrolledNumber operator +(UncontrolledNumber a, UncontrolledNumber b)
        {
            if (a.IsNegative != b.IsNegative && b.IsNegative) return b + a;
            bool isNegative = false;
            BigInteger integer = a.Integer + b.Integer;
            (BigInteger _decimal, BigInteger bFrac) = MatchDecimalDigits(a, b);
            (int aDigits, int bDigits) = (GetDigitCount(_decimal), GetDigitCount(bFrac));

            // Case 1: both positive or both negative (matching sign).
            if (a.IsNegative == b.IsNegative)
            {
                _decimal += bFrac;
                int newDigitCount = GetDigitCount(_decimal);
                if (newDigitCount > aDigits)
                {
                    // Add 1 to integer and remove new digit from _decimal
                    integer += a.IsNegative ? -1 : 1;
                    _decimal -= (_decimal.ToString()[0] - '0') * BigInteger.Pow(10, newDigitCount - 1);
                }
            }
            else if (a.IsNegative) // Case 2: a is negative and b is positive.
            {
                isNegative = b < a;
                // take _decimal.
                _decimal -= bFrac;
                if (_decimal.Sign < 0 && integer.Sign < 0)
                {
                    int digitCount = GetDigitCount(-_decimal);
                    integer += 1;
                    _decimal = BigInteger.Pow(10, digitCount) + _decimal;
                }
            }

            return new UncontrolledNumber(integer, _decimal, Math.Min(a.DecimalZeroes, b.DecimalZeroes), isNegative);
        }
        public static UncontrolledNumber operator -(UncontrolledNumber a, UncontrolledNumber b) => a + (-b);

        public static UncontrolledNumber operator *(UncontrolledNumber a, UncontrolledNumber b)
        {
            if (a.IsInfinity && !b.IsZero || b.IsInfinity && !a.IsZero) return Infinity;
            // Multiply integers
            bool isNegative = a.IsNegative != b.IsNegative;
            BigInteger completeInteger = (a.IsNegative ? -a.CompleteInteger : a.CompleteInteger)
                                        * (b.IsNegative ? -b.CompleteInteger : b.CompleteInteger);
            BigInteger _decimal = 0;
            int decimals = a.DecimalDigits + a.DecimalZeroes + b.DecimalDigits + b.DecimalZeroes;
            int zeroes = 0;
            while (decimals > 0)
            {
                _decimal *= 10;
                BigInteger digit = completeInteger % 10;
                if (digit == 0)
                {
                    zeroes++;
                }
                else
                {
                    zeroes = 0;
                }
                _decimal += digit;
                decimals--;
                completeInteger /= 10;
            }
            if (_decimal == 0) zeroes = 0;
            return new UncontrolledNumber(isNegative ? -completeInteger : completeInteger, ReversePositiveInteger(_decimal), zeroes, isNegative);
        }

        /// <summary>
        /// Controls how accurate divisions are for the number.
        /// Since divisions can result in irrational numbers, this limits the amount of decimals
        /// that can result in an division.
        /// </summary>
        public static int DivisionAccuracy = 30;
        public static UncontrolledNumber operator /(UncontrolledNumber a, UncontrolledNumber b)
        {
            if (b.IsZero) return Infinity;
            if (a.IsZero || b.IsInfinity) return new UncontrolledNumber();
            bool isNegative = a.IsNegative != b.IsNegative;
            (BigInteger ac, BigInteger bc) = GetMatchedCompleteInteger(a, b);
            BigInteger rm = 0;
            BigInteger completeInteger = BigInteger.DivRem(ac, bc, out rm);
            BigInteger amp = BigInteger.Pow(10, DivisionAccuracy - 1);
            BigInteger divider = (amp * rm);
            BigInteger _decimal = (divider / bc);
            int zeroes = DivisionAccuracy - 1 - GetDigitCount(_decimal); // difference between division length.
            if (zeroes < 0) zeroes = 0;
            return new UncontrolledNumber(completeInteger, _decimal, zeroes, isNegative);
        }

        public static UncontrolledNumber operator %(UncontrolledNumber a, UncontrolledNumber b)
        {
            if (b.IsZero) return a;
            if (a.IsInfinity) return new UncontrolledNumber();
            UncontrolledNumber whole = a / b; // e.g. 400 / 300 = 1.33333... 400 % 300 = 100
            if (whole.IsNegative) whole = -whole;
            // Remove decimal
            whole.Decimal = 0;
            whole.DecimalDigits = 0;
            whole.DecimalZeroes = 0;

            UncontrolledNumber baseNumber = b * whole; // e.g. 300 * 1 = 300 

            return a - baseNumber; // e.g. 400 - 300 = 100
        }

        public static UncontrolledNumber operator ^(UncontrolledNumber a, UncontrolledNumber b)
        {
            return new UncontrolledNumber(a.Integer ^ b.Integer, 0, 0, false);
        }

        public static UncontrolledNumber operator |(UncontrolledNumber a, UncontrolledNumber b)
        {
            return new UncontrolledNumber(a.Integer | b.Integer, 0, 0, false);
        }

        public static UncontrolledNumber operator &(UncontrolledNumber a, UncontrolledNumber b)
        {
            return new UncontrolledNumber(a.Integer & b.Integer, 0, 0, false);
        }

        public static bool operator ==(UncontrolledNumber a, UncontrolledNumber b)
        {
            return a.IsNegative == b.IsNegative && a.Integer == b.Integer && a.Decimal == b.Decimal && a.IsInfinity == b.IsInfinity;
        }

        public static bool operator !=(UncontrolledNumber a, UncontrolledNumber b)
        {
            return a.IsNegative != b.IsNegative && a.Integer != b.Integer || a.Decimal != b.Decimal || a.IsInfinity != b.IsInfinity;
        }

        public static bool operator >(UncontrolledNumber a, UncontrolledNumber b)
        {
            return !(a <= b);
        }
        public static bool operator >=(UncontrolledNumber a, UncontrolledNumber b)
        {
            return (a == b || a > b);
        }
        public static bool operator <(UncontrolledNumber a, UncontrolledNumber b)
        {
            if (a.IsInfinity || b.IsInfinity)
            {
                int infA = (a.IsNegative ? -1 : 1) * (a.IsInfinity ? 1 : 0);
                int infB = (b.IsNegative ? -1 : 1) * (b.IsInfinity ? 1 : 0);
                if (infA != infB)
                {
                    return infA < infB;
                }
            }
            if (a.IsNegative && !b.IsNegative) return true;
            if (!a.IsNegative && b.IsNegative) return false;
            return a.Integer < b.Integer || a.Integer == b.Integer && CompareDecimal(a, b) == (a.IsNegative && b.IsNegative ? -1 : 1);
        }
        public static bool operator <=(UncontrolledNumber a, UncontrolledNumber b)
        {
            return (a == b || a < b);
        }
        #endregion
        #endregion
        public override string ToString()
        {
            if (IsInfinity) return "inf";
            string neg = Integer.Sign != -1 && IsNegative ? "-" : "";
            if (Decimal == 0) return neg + $"{Integer}";
            string dec = $"{Integer}." + "".PadLeft(DecimalZeroes, '0') + $"{Decimal}";
            return neg + dec;
        }

        /// <summary>
        /// Converts the number back into a double.
        /// </summary>
        /// <returns></returns>
        public double ToDouble()
        {
            if (IsInfinity) return IsNegative ? double.NegativeInfinity : double.PositiveInfinity;
            return double.Parse(ToString());
        }

        public override bool Equals(object obj)
        {
            return ((obj is UncontrolledNumber && ((UncontrolledNumber)obj == this)));
        }

        public override int GetHashCode()
        {
            return Integer.GetHashCode();
        }

        public int CompareTo(object? obj)
        {
            if (!(obj is UncontrolledNumber)) return int.MinValue;

            UncontrolledNumber other = (UncontrolledNumber)obj;
            return other < this ? 1 : other == this ? 0 : -1;
        }
    }
}