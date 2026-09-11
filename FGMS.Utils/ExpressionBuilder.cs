using System.Linq.Expressions;

namespace FGMS.Utils
{
    //public static class ExpressionBuilder
    //{
    //    public static Expression<Func<T, bool>> GetTrue<T>() { return f => true; }

    //    public static Expression<Func<T, bool>> GetFalse<T>() { return f => false; }

    //    public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> expr, bool condition, Expression<Func<T, bool>> predicate)
    //    {
    //        return condition ? expr.And(predicate) : expr;
    //    }

    //    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    //    {
    //        return first.Compose(second, Expression.And);
    //    }

    //    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    //    {
    //        return first.Compose(second, Expression.Or);
    //    }

    //    private static Expression<Func<T, bool>> Compose<T>(
    //        this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2, Func<Expression, Expression, BinaryExpression> func)
    //    {
    //        var parameter = Expression.Parameter(typeof(T));
    //        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
    //        var left = leftVisitor.Visit(expr1.Body);
    //        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
    //        var right = rightVisitor.Visit(expr2.Body);
    //        return Expression.Lambda<Func<T, bool>>(func(left, right), parameter);
    //    }

    //    private class ReplaceExpressionVisitor : ExpressionVisitor
    //    {
    //        private readonly Expression _oldValue;
    //        private readonly Expression _newValue;

    //        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
    //        {
    //            _oldValue = oldValue;
    //            _newValue = newValue;
    //        }

    //        public override Expression Visit(Expression node)
    //        {
    //            if (node == _oldValue)
    //                return _newValue;
    //            return base.Visit(node);
    //        }
    //    }
    //}

    public static class ExpressionBuilder
    {
        public static Expression<Func<T, bool>> GetTrue<T>() { return f => true; }

        public static Expression<Func<T, bool>> GetFalse<T>() { return f => false; }

        // 原有的 AndIf
        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> expr, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? expr.And(predicate) : expr;
        }

        // 新增的 OrIf
        public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> expr, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? expr.Or(predicate) : expr;
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

        // 可选：扩展 AndIf 和 OrIf 的重载，支持多个条件的组合判断
        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> expr, Func<bool> conditionFactory, Expression<Func<T, bool>> predicate)
        {
            return conditionFactory() ? expr.And(predicate) : expr;
        }

        public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> expr, Func<bool> conditionFactory, Expression<Func<T, bool>> predicate)
        {
            return conditionFactory() ? expr.Or(predicate) : expr;
        }

        private static Expression<Func<T, bool>> Compose<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2, Func<Expression, Expression, BinaryExpression> func)
        {
            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);
            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(func(left, right), parameter);
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}
