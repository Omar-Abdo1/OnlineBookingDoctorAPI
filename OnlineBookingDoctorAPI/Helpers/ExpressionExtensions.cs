using System;
using System.Linq.Expressions;

namespace OnlineBookingAPI.Helpers;

public static class ExpressionExtensions
{
public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left, 
        Expression<Func<T, bool>> right)
    {
        // 1. Create a parameter map to ensure the 'p' in both expressions refers to the same object
        var invokedExpr = Expression.Invoke(right, left.Parameters.Cast<Expression>());
        
        // 2. Combine the two expressions using Expression.AndAlso (which translates to SQL AND)
        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(left.Body, invokedExpr), left.Parameters);
    }
}
