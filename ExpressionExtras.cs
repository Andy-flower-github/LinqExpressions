using System;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;

namespace Andy.Data
{
    public struct MyExpression<TEntity>
    {
        public Expression BodyExpr { get; set; }
        public ParameterExpression ParamExpr { get; }
        public Expression<Func<TEntity, bool>> GetLambda() => Expression.Lambda<Func<TEntity, bool>>(BodyExpr, ParamExpr);

        public MyExpression(string name = "x")
        {
            BodyExpr = null;
            ParamExpr = Expression.Parameter(typeof(TEntity), name);
        }
    }

    public static class ExpressionExtras
    {
        private static bool HasValue(this object sender) => !(sender == null || string.IsNullOrWhiteSpace(sender.ToString()));

        #region MaybeAnd
        /// <summary>
        /// Andy -> 當 grandtrue = True 才會產生 Expression.And   -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> MaybeAnd<T>(this MyExpression<T> left, bool grandtrue
            , Func<ParameterExpression, string, object, Expression> doFunc, string property, object value)
            => MaybeAnd(left, grandtrue ? doFunc(left.ParamExpr, property, value) : null);

        /// <summary>
        /// Andy -> 判斷 right.value 有值才會產生 Expression.And. -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> MaybeAnd<T>(this MyExpression<T> left
            , Func<ParameterExpression, string, object, Expression> doFunc, string property, object value)
            => MaybeAnd(left, value.HasValue(), doFunc, property, value);

        /// <summary>
        /// Andy -> 一律會產生 Expression.And. -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> And<T>(this MyExpression<T> left
            , Func<ParameterExpression, string, object, Expression> doFunc, string property, object value)
            => MaybeAnd(left, true, doFunc, property, value);

        /// <summary>
        /// Andy -> 判斷產生沒有 Right.Value 的 Expression.And.  -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> And<T>(this MyExpression<T> left
             , Func<ParameterExpression, string, Expression> doFunc, string property)
             => MaybeAnd(left, doFunc(left.ParamExpr, property));

        /// <summary>
        /// Andy -> 使用自定的 Func 來取得 Expression.And.  -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> And<T>(this MyExpression<T> left, Func<MyExpression<T>, Expression> dofunc)
        {
            var newExpression = left;
            newExpression.BodyExpr = null;
            return MaybeAnd(left, dofunc.Invoke(newExpression));
        }

        /// <summary>
        /// Andy -> 當 grandtrue = True 才會使用自定的 Func 來取得 Expression.  -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> MaybeAnd<T>(this MyExpression<T> left, bool grandtrue, Func<MyExpression<T>, Expression> dofunc)
        {
            if (!grandtrue)
                return left;

            var newExpression = left;
            newExpression.BodyExpr = null;
            return MaybeAnd(left, dofunc.Invoke(newExpression));
        }

        /// <summary>
        /// Andy -> 只要是 and / MaybeAnd 的多載最後會導到這裡
        /// </summary>
        public static MyExpression<T> MaybeAnd<T>(this MyExpression<T> left, Expression right)
        {
            if (right == null)
                return left;

            left.BodyExpr = left.BodyExpr == null ? right : Expression.And(left.BodyExpr, right);
            return left;
        }
        #endregion

        #region MayOr
        /// <summary>
        /// Andy -> 當 grandtrue = True 才會產生 Expression.Or; -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> MaybeOr<T>(this MyExpression<T> left, bool istrue
            , Func<ParameterExpression, string, object, Expression> doFunc, string property, object value)
            => MaybeOr(left, istrue ? doFunc(left.ParamExpr, property, value) : null);

        /// <summary>
        /// Andy -> 判斷 right.value 有值才會產生 Expression.Or; -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> MaybeOr<T>(this MyExpression<T> left
            , Func<ParameterExpression, string, object, Expression> doFunc, string property, object value)
            => MaybeOr(left, value.HasValue(), doFunc, property, value);

        /// <summary>
        /// Andy -> 一律會產生 Expression.Or; -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> Or<T>(this MyExpression<T> left
            , Func<ParameterExpression, string, object, Expression> doFunc, string property, object value)
            => MaybeOr(left, true, doFunc, property, value);

        public static MyExpression<T> Or<T>(this MyExpression<T> left
            , Func<ParameterExpression, string, Expression> doFunc, string property)
            => MaybeOr(left, doFunc(left.ParamExpr, property));

        /// <summary>
        /// Andy -> 當 grandtrue = True 才會使用自定的 Func 來取得 Expression.  -- doFunc 可以利用 Expr.{Fnuc} 來取得
        /// </summary>
        public static MyExpression<T> MaybeOr<T>(this MyExpression<T> left, bool grandtrue, Func<MyExpression<T>, Expression> dofunc)
        {
            if (!grandtrue)
                return left;
            var newExpression = left;
            newExpression.BodyExpr = null;
            return MaybeOr(left, dofunc.Invoke(newExpression));
        }

        /// <summary>
        /// Andy -> 只要是 Or / MaybeOr 的多載最後會導到這裡
        /// </summary>
        public static MyExpression<T> MaybeOr<T>(this MyExpression<T> left, Expression right)
        {
            if (right == null)
                return left;

            left.BodyExpr = left.BodyExpr == null ? right : Expression.Or(left.BodyExpr, right);
            return left;
        }
        #endregion
    }

    public class Expr
    {
        /// <summary>
        /// Andy => 建立並回傳 MyExpression 的 instance
        /// </summary>
        public static MyExpression<TEntity> CreateMyExpression<TEntity>(string name = "x")
            => new MyExpression<TEntity>(name);

        /// <summary>
        /// Andy => 建立並回傳 MyExpression 的 instance
        /// </summary>
        public static MyExpression<TEntity> CreateMyExpression<TEntity>(string name, out ParameterExpression parameter)
        {
            var express = new MyExpression<TEntity>(name);
            parameter = express.ParamExpr;
            return express;
        }

        /// <summary>
        /// Andy -> 取得左邊的 MemberExpression. 例如: X.CustomerID
        /// </summary>
        private static MemberExpression Left(ParameterExpression paramater, string property)
            => Expression.Property(paramater, property);

        /// <summary>
        /// Andy -> 取得右邊的 ConstantExpression. 例如: "A001", 500, "2020-12-31"
        /// </summary>
        private static ConstantExpression Right(ParameterExpression paramater, string property, object value)
            => Expression.Constant(value, Left(paramater, property).Type);

        /// <summary>
        /// Andy -> x.Left ＝ Right
        /// </summary>
        public static Expression Equal(ParameterExpression paramater, string property, object value)
            => Expression.Equal(Left(paramater, property), Right(paramater, property, value));

        /// <summary>
        /// Andy -> x.Left ＝ null
        /// </summary>
        public static Expression IsNull(ParameterExpression paramater, string property)
            => Expression.Equal(Left(paramater, property), Right(paramater, property, null));

        /// <summary>
        /// Andy -> x.Left !＝ null
        /// </summary>
        public static Expression IsNotNull(ParameterExpression paramater, string property)
            => Expression.NotEqual(Left(paramater, property), Right(paramater, property, null));

        /// <summary>
        /// Andy -> x.Left != Right
        /// </summary>
        public static Expression NotEqual(ParameterExpression paramater, string property, object value)
            => Expression.NotEqual(Left(paramater, property), Right(paramater, property, value));

        /// <summary>
        /// Andy -> 文字 => x.left.CompareTo(right)＞0; 如果是Right欄位名稱 =>  x.Left > x.Right; 數字或日期 => x.Left＞Right 
        /// </summary>
        public static Expression GreaterThan(ParameterExpression paramater, string property, object value)
            => Left(paramater, property).Type == typeof(String)
                ? Expression.GreaterThan(CompareTo(paramater, property, value), Expression.Constant(0))
                : value.GetType() == typeof(String)
                    ? Expression.GreaterThan(Left(paramater, property), Left(paramater, (string)value))
                    : Expression.GreaterThan(Left(paramater, property), Right(paramater, property, value));

        /// <summary>
        /// Andy -> 文字 => x.left.CompareTo(right)＞＝0; 數字或日期 => x.Left＞＝right, 如果是Right欄位名稱 =>  x.Left > x.Right 
        /// </summary>
        public static Expression GreaterThanOrEqual(ParameterExpression paramater, string property, object value)
            => Left(paramater, property).Type == typeof(String)
                ? Expression.GreaterThanOrEqual(CompareTo(paramater, property, value), Expression.Constant(0))
                : value.GetType() == typeof(String)
                    ? Expression.GreaterThanOrEqual(Left(paramater, property), Left(paramater, (string)value))
                    : Expression.GreaterThanOrEqual(Left(paramater, property), Right(paramater, property, value));

        /// <summary>
        /// Andy -> 文字 => x.Left.CompareTo(Right) ＜ 0; 數字或日期 => x.Left ＜ (x.)right
        /// </summary>
        public static Expression LessThan(ParameterExpression paramater, string property, object value)
           => Left(paramater, property).Type == typeof(String)
                ? Expression.LessThan(CompareTo(paramater, property, value), Expression.Constant(0))
                : value.GetType() == typeof(String)
                    ? Expression.LessThan(Left(paramater, property), Left(paramater, (string)value))
                    : Expression.LessThan(Left(paramater, property), Right(paramater, property, value));

        /// <summary>
        /// Andy -> 文字 => x.Left.CompareTo(Right) ＜＝ 0; 數字或日期 => x.Left ＜＝ Right
        /// </summary>
        public static Expression LessThanOrEqual(ParameterExpression paramater, string property, object value)
             => Left(paramater, property).Type == typeof(String)
                ? Expression.LessThanOrEqual(CompareTo(paramater, property, value), Expression.Constant(0))
                : value.GetType() == typeof(String)
                    ? Expression.LessThanOrEqual(Left(paramater, property), Left(paramater, (string)value))
                    : Expression.LessThanOrEqual(Left(paramater, property), Right(paramater, property, value));

        /// <summary>
        /// Andy -> x.Pay1 + x.Pay2  or  x.Pay1 + 200
        /// </summary>
        public static Expression Add(ParameterExpression paramater, string property, object value)
            => value.GetType() == typeof(String)
                ? Expression.Add(Left(paramater, property), Left(paramater, (string)value))
                : Expression.Add(Left(paramater, property), Right(paramater, property, value));

        /// <summary>
        /// Andy -> x.Pay1 - x.Pay2  or  x.Pay1 - 200
        /// </summary>
        public static Expression Subtract(ParameterExpression paramater, string property, object value)
            => value.GetType() == typeof(String)
                ? Expression.Subtract(Left(paramater, property), Left(paramater, (string)value))
                : Expression.Subtract(Left(paramater, property), Right(paramater, property, value));

        #region Call Function
        /// <summary>
        /// Andy -> x.Left.Contains(Right)
        /// </summary>
        public static Expression Contains(ParameterExpression paramater, string property, object value)
           => Expression.Call(Left(paramater, property), GetMethodInfo<String>(nameof(string.Contains)), Right(paramater, property, value));

        /// <summary>
        /// Andy -> x.Left.StartsWith(Riht)
        /// </summary>
        public static Expression StartsWith(ParameterExpression paramater, string property, object value)
           => Expression.Call(Left(paramater, property), GetMethodInfo<String>(nameof(string.StartsWith)), Right(paramater, property, value));

        /// <summary>
        /// Andy -> x.Left.EndsWith(Riht)
        /// </summary>
        public static Expression EndsWith(ParameterExpression paramater, string property, object value)
           => Expression.Call(Left(paramater, property), GetMethodInfo<String>(nameof(string.EndsWith)), Right(paramater, property, value));

        /// <summary>
        /// Andy -> x.Left.CompareTo(Riht)
        /// </summary>
        public static Expression CompareTo(ParameterExpression paramater, string property, object comparevalue)
           => Expression.Call(Left(paramater, property), GetMethodInfo<String>(nameof(string.CompareTo)), Right(paramater, property, comparevalue));
        #endregion

        /// <summary>
        /// Andy -> 利用 Function Name 取得指定型別 T 的 MethodInfo
        /// </summary>
        public static MethodInfo GetMethodInfo<T>(string name) => typeof(T).GetMethod(name, new Type[] { typeof(T) });
    }
}
