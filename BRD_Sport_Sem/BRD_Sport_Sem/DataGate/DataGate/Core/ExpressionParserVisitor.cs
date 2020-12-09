using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using DataGate.Utils;
using Npgsql;

namespace DataGate.Core
{
    //DONE:
    // - All logical and basic arithmetics operators
    // - Support for Nullable Types(.Value and .HasValue supported)
    // - Support for Converting and ToString (translates to CAST operator)
    // - Support for concatenation operator
    // - Support for constants
    // - Support for closure and external variables (translates to command params with set values)
    //     - Invariant for case with equal names
    // - Support for parameter of lambda (translates to registered table and given identifier for field)
    // - Support of string comparision operations (Contains, StartsWith, EndsWith)
    
    
    // Does NOT support comparing parameter of lambda with object of the same type
    public class ExpressionParserVisitor
    {
        private NpgsqlCommand _command;
        private int depth;

        private Dictionary<MemberInfo, string> _usedVariables;
        private Dictionary<string, int> _usedVariableNames;

        public HashSet<Type> TypesUsed;

        public ExpressionParserVisitor(NpgsqlCommand command)
        {
            _command = command;
            TypesUsed = new HashSet<Type>();
            _usedVariables = new Dictionary<MemberInfo, string>();
            _usedVariableNames = new Dictionary<string, int>();
        }

        public string Parse(Expression expression)
        {
            depth = -1;
            
            var reducedExression = expression;
            while (reducedExression.CanReduce)
                reducedExression = expression.ReduceAndCheck();
            var commandText = Visit(reducedExression, false);
            return commandText;
        }

        private string Visit(Expression expression, bool isParenthesisNeeded = true)
        {
            depth++;

            var result = "";
            if (expression is BinaryExpression binaryExpression)
                result = VisitBinary(binaryExpression, isParenthesisNeeded);

            else if (expression is ConstantExpression constantExpression)
                result = VisitConstant(constantExpression);

            else if (expression is MemberExpression memberExpression)
                result = VisitMember(memberExpression);
            
            else if (expression is UnaryExpression unaryExpression)
                result = VisitUnary(unaryExpression);
            
            else if (expression is MethodCallExpression methodCallExpression)
                result = VisitMethodCall(methodCallExpression, isParenthesisNeeded);


            depth--;
            
            return result;
        }

        private string VisitBinary(BinaryExpression binaryExpression, bool isParenthesisNeeded = true)
        {
            var builder = new StringBuilder();

            builder.Append($"{Visit(binaryExpression.Left)}");

            switch (binaryExpression.NodeType)
            {
                #region Logical
                
                case ExpressionType.AndAlso:
                    builder.Append(" AND ");
                    break;
                
                case ExpressionType.OrElse:
                    builder.Append(" OR ");
                    break;
                
                case ExpressionType.Equal:
                    if (binaryExpression.Left is ConstantExpression leftConst
                        && leftConst.Value == null
                        || binaryExpression.Right is ConstantExpression rightConst
                        && rightConst.Value == null)
                        builder.Append(" IS NOT DISTINCT FROM ");
                    else builder.Append(" = ");
                    break;
                    
                case ExpressionType.NotEqual:
                    if (binaryExpression.Left is ConstantExpression leftConst2
                        && leftConst2.Value == null
                        || binaryExpression.Right is ConstantExpression rightConst2
                        && rightConst2.Value == null)
                        builder.Append(" IS DISTINCT FROM ");
                    else builder.Append(" <> ");
                    break;
                
                case ExpressionType.LessThan:
                    builder.Append(" < ");
                    break;
                
                case ExpressionType.GreaterThan:
                    builder.Append(" > ");
                    break;
                
                case ExpressionType.LessThanOrEqual:
                    builder.Append(" <= ");
                    break;
                
                case ExpressionType.GreaterThanOrEqual:
                    builder.Append(" >= ");
                    break;
                
                #endregion
                
                #region Simple
                
                case ExpressionType.Add:
                    if (binaryExpression.Type == typeof(string))
                        builder.Append(" || ");
                    else builder.Append(" + ");
                    break;
                
                case ExpressionType.Subtract:
                    builder.Append(" - ");
                    break;
                    
                case ExpressionType.Divide:
                    builder.Append(" / ");
                    break;
                
                case ExpressionType.Multiply:
                    builder.Append(" * ");
                    break;
                
                #endregion
            }
            
            builder.Append($"{Visit(binaryExpression.Right)}");

            string result = null;
            if (isParenthesisNeeded)
                result = $"({builder})";
            else result = builder.ToString();
            
            return result;
        }

        private string MapType(Type type)
        {
            if (type == typeof(bool))
                return "boolean";
            if (type == typeof(string))
                return "text";
            if (type == typeof(DateTime))
                return "date";
            if (type == typeof(TimeSpan))
                return "timestamp";

            return null;
        }

        private string VisitUnary(UnaryExpression unaryExpression)
        {
            var builder = new StringBuilder();
            
            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Not:
                    builder.Append("NOT ");
                    builder.Append($"({Visit(unaryExpression.Operand, false)})");
                    break;
                
                case ExpressionType.Convert:
                    var mappedType = MapType(unaryExpression.Type);
                    if (mappedType == null)
                        return Visit(unaryExpression.Operand);
                    builder.Append("CAST (");
                    builder.Append($"{Visit(unaryExpression.Operand)}");
                    builder.Append($" AS {mappedType})");
                    break;
                
                case ExpressionType.Negate:
                    builder.Append("-");
                    builder.Append($"{Visit(unaryExpression.Operand)}");
                    break;
            }
            
            return builder.ToString();
        }

        private string VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Type == typeof(int) ||
                constantExpression.Type == typeof(long) ||
                constantExpression.Type == typeof(double) ||
                constantExpression.Type == typeof(float) ||
                constantExpression.Type == typeof(decimal) ||
                constantExpression.Type == typeof(byte))
                return constantExpression.Value.ToString();

            if (constantExpression.Value == null)
                return "NULL";
            
            return $"'{constantExpression.Value}'";
        }

        private object GetValue(Expression expression)
        {
            if (expression is ConstantExpression constantExpression)
                return constantExpression.Value;

            if (expression is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                var obj = GetValue(memberExpression.Expression);
                return member.ToVariableInfo().Get(obj);
            }

            return null;
        }

        private string VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.NodeType == ExpressionType.MemberAccess)
            {
                Expression rootExpression = memberExpression;
                while (rootExpression is MemberExpression rootMember &&
                       rootMember.NodeType == ExpressionType.MemberAccess)
                    rootExpression = rootMember.Expression;
                
                if (rootExpression is ConstantExpression constantExpression) //External variable
                {
                    var value = GetValue(memberExpression);
                    var variableName = memberExpression.Member.Name;
                    if (!_usedVariables.ContainsKey(memberExpression.Member))
                    {
                        if (_usedVariableNames.ContainsKey(variableName))
                            variableName = $"{variableName}_{++_usedVariableNames[variableName]}";
                        else _usedVariableNames[variableName] = 1;

                        _usedVariables.Add(memberExpression.Member, variableName);
                        _command.Parameters.AddWithValue(variableName, value ?? DBNull.Value);
                    }
                    else variableName = _usedVariables[memberExpression.Member];

                    return $"@{variableName}";
                }

                if (DataGateORM.ConnectionContext.Registry.Contains(memberExpression.Expression.Type))
                {
                    var paramType = memberExpression.Expression.Type;
                    TypesUsed.Add(paramType);
                    var relationship = DataGateORM.ConnectionContext.Registry.GetRelationship(paramType);
                    return $"{relationship.TableName}.{TableTypeRelationship.GetDbIdentifier(memberExpression.Member)}";
                }

                if (memberExpression.Member == ReflectionFinder.GetMemberInfoSingle(typeof(Nullable<>), "HasValue")) //Nullable type
                    return Visit(Expression.NotEqual(memberExpression.Expression, Expression.Constant(null)));

                if (memberExpression.Member == ReflectionFinder.GetMemberInfoSingle<string>(nameof(string.Length)))
                    return $"char_length({Visit(memberExpression.Expression, false)})";
                    
                return Visit(memberExpression.Expression);
            }

            return "";
        }

        private string VisitMethodCall(MethodCallExpression methodCallExpression, bool isParenthesisNeeded = true)
        {
            var method = methodCallExpression.Method;
            
            if (method.Name == "ToString")
                return Visit(Expression.Convert(methodCallExpression.Object, typeof(string)));

            if (method.DeclaringType == typeof(string))
            {
                if (method.Name == nameof(string.Contains) 
                    || method.Name == nameof(string.StartsWith)
                    || method.Name == nameof(string.EndsWith))
                {
                    var builder = new StringBuilder();
                    builder.Append(Visit(methodCallExpression.Object));
                    builder.Append(" LIKE ");

                    if (method.Name == nameof(string.StartsWith)
                        || method.Name == nameof(string.Contains))
                        builder.Append("'%' || ");
                    
                    builder.Append(Visit(methodCallExpression.Arguments.Single()));
                    
                    if (method.Name == nameof(string.EndsWith)
                        || method.Name == nameof(string.Contains))
                        builder.Append(" || '%'");
                    
                    if (isParenthesisNeeded)
                        return $"({builder})";
                    return builder.ToString();
                }
                
                if (method == ReflectionFinder.GetMemberInfoSingle<string>(nameof(string.ToLower)))
                    return $"lower({Visit(methodCallExpression.Object, false)})";
                
                if (method == ReflectionFinder.GetMemberInfoSingle<string>(nameof(string.ToUpper)))
                    return $"upper({Visit(methodCallExpression.Object, false)})";
            }

            return "";
        }
    }
}