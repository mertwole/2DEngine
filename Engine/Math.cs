using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace Engine
{
    public abstract class Math
    {
        #region funcs
        public static float min(float first, float second)
        {
            if(first > second)
            { return second; }
            return first;
        }

        public static float min(float first, float second, float third, float fourth)
        {
            return min(min(first, second), min(third, fourth));
        }

        public static float max(float first, float second)
        {
            if (first < second)
            { return second; }
            return first;
        }

        public static float max(float first, float second, float third, float fourth)
        {
            return max(max(first, second), max(third, fourth));
        }

        public static int sgn(float num)
        {
            if (num == 0)
            { return 0; };


            if(num > 0)
            { return 1; }

            return -1;
        }

        public static float sqr(float num)
        {
            return num * num;
        }

        public static float abs(float num)
        {
            if (num < 0)
            { return -num; }

            return num;
        }

        public static float sqrt(float num)
        {
            if (num == 0)
                return 0;

            float g = num;
            while (true)
            {
                float t = (num / g + g) / 2;
                if (abs(g - t) < 1E-10)
                {
                    return g;
                }
                g = t;
            }
        }
        #endregion

        public struct Vector2
        {
            public float x;
            public float y;

            public static readonly Vector2 zero = new Vector2(0, 0);

            public float sqrMagnitude { get { return x * x + y * y; } }

            public float magnitude { get { return sqrt(sqrMagnitude); } }

            public Vector2 normalized { get { return (x == 0 && y == 0)? zero : this / magnitude; } }

            public Vector2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public static Vector2 operator +(Vector2 first, Vector2 second)
            {
                return new Vector2(first.x + second.x, first.y + second.y);
            }

            public static Vector2 operator -(Vector2 first, Vector2 second)
            {
                return new Vector2(first.x - second.x, first.y - second.y);
            }

            public static Vector2 operator -(Vector2 vec)
            {
                return new Vector2(-vec.x, -vec.y);
            }

            public static Vector2 operator *(Vector2 vector, float k)
            {
                return new Vector2(vector.x * k, vector.y * k);
            }

            public static Vector2 operator *(float k, Vector2 vector)
            {
                return new Vector2(vector.x * k, vector.y * k);
            }

            public static float operator *(Vector2 first, Vector2 second)
            {
                return first.x * second.x + first.y * second.y;
            }

            public static Vector2 operator /(Vector2 vector, float k)
            {
                return new Vector2(vector.x / k, vector.y / k);
            }

   


            public static float CrossProduct(Vector2 first, Vector2 second)
            {
                return first.x * second.y - first.y * second.x;
            }

            public static Vector2 CrossProduct(float first, Vector2 second)
            {
                return new Vector2(-first * second.y, first * second.x);
            }

            public static Vector2 CrossProduct(Vector2 first, float second)
            {
                return new Vector2(second * first.y, -second * first.x);
            }

        }

        public struct Matrix2x2
        {
            public static Matrix2x2 identity = new Matrix2x2(1, 0, 0, 1);

            float[,] matrix;

            public float this[int x, int y]
            {
                get { return matrix[x, y]; }
                set { matrix[x, y] = value; }
            }

            #region constructors
            public Matrix2x2(float num00, float num10, float num01, float num11)
            {
                matrix = new float[2, 2];

                matrix[0, 0] = num00;
                matrix[1, 0] = num10;
                matrix[0, 1] = num01;
                matrix[1, 1] = num11;
            }

            public Matrix2x2(Vector2 top, Vector2 bottom)
            {
                matrix = new float[2, 2];

                matrix[0, 0] = top.x;
                matrix[1, 0] = top.y;
                matrix[0, 1] = bottom.x;
                matrix[1, 1] = bottom.y;
            }

            public enum MatrixCreationType { rotation_rad, rotation_deg };

            public Matrix2x2(float num, MatrixCreationType creationtype)
            {
                matrix = new float[2, 2];

                switch (creationtype)
                {
                    case MatrixCreationType.rotation_rad:
                        {
                            float sin = (float)Sin(num);
                            float cos = (float)Cos(num);

                            matrix[0, 0] = cos;
                            matrix[1, 0] = -sin;
                            matrix[0, 1] = sin;
                            matrix[1, 1] = cos;

                            return;
                        }
                    case MatrixCreationType.rotation_deg:
                        {
                            double inrad = num / 180 * PI;

                            float sin = (float)Sin(inrad);
                            float cos = (float)Cos(inrad);

                            matrix[0, 0] = cos;
                            matrix[1, 0] = -sin;
                            matrix[0, 1] = sin;
                            matrix[1, 1] = cos;

                            return;
                        }
                }

            }
            #endregion

            public static Vector2 operator *(Matrix2x2 mat, Vector2 vec)
            {
                return new Vector2( mat[0, 0] * vec.x + mat[1, 0] * vec.y,
                                    mat[0, 1] * vec.x + mat[1, 1] * vec.y);
            }

            public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b)
            {
                return new Matrix2x2(
                    new Vector2(/**/a[0, 0] * b[0, 0] + a[1, 0] * b[0, 1]/**/, /**/a[0, 0] * b[1, 0] + a[1, 0] * b[1, 1]/**/),
                    new Vector2(/**/a[0, 1] * b[0, 0] + a[1, 1] * b[0, 1]/**/, /**/a[0, 1] * b[1, 0] + a[1, 1] * b[1, 1]/**/));
            }

            public static Matrix2x2 operator *(Matrix2x2 mat, float num)
            {
                return new Matrix2x2(mat[0, 0] * num, mat[1, 0] * num, mat[0, 1] * num, mat[1, 1] * num);
            }

            public static Matrix2x2 operator *(float num, Matrix2x2 mat)
            {
                return new Matrix2x2(mat[0, 0] * num, mat[1, 0] * num, mat[0, 1] * num, mat[1, 1] * num);
            }

        }
    }
}
