using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Npgsql;

namespace DataGate.Core
{
    public enum OrderByType
    {
        Ascending,
        Descending
    }
    
    public class QueryBuilder<TIn, TOut>
    {
        private DataGateORM _orm;
        
        private Expression _whereExpression;
        private HashSet<Type> _fromParams;

        private int? _offset;
        private int? _limit;
        private bool _distinct = false;

        private List<Tuple<Expression, OrderByType>> _orderByExpressions;
        private List<Tuple<Expression, Type>> _joinExpressions;

        public QueryBuilder<TIn, TOut> Clone()
        {
            var queryBuilder = new QueryBuilder<TIn, TOut>(_orm);
            queryBuilder._whereExpression = _whereExpression;
            queryBuilder._distinct = _distinct;
            queryBuilder._limit = _limit;
            queryBuilder._offset = _offset;
            queryBuilder._orderByExpressions = _orderByExpressions;

            queryBuilder._joinExpressions = _joinExpressions;
            queryBuilder._fromParams = _fromParams;
            return queryBuilder;
        }

        public QueryBuilder<TIn2, TOut> ConvertInput<TIn2>()
        {
            var queryBuilder = new QueryBuilder<TIn2, TOut>(_orm);
            queryBuilder._whereExpression = _whereExpression;
            queryBuilder._distinct = _distinct;
            queryBuilder._limit = _limit;
            queryBuilder._offset = _offset;
            queryBuilder._orderByExpressions = _orderByExpressions;

            queryBuilder._joinExpressions = _joinExpressions;
            queryBuilder._fromParams = _fromParams;
            return queryBuilder;
        }
        
        public QueryBuilder<TIn, TOut2> ConvertOutput<TOut2>()
        {
            var queryBuilder = new QueryBuilder<TIn, TOut2>(_orm);
            queryBuilder._whereExpression = _whereExpression;
            queryBuilder._distinct = _distinct;
            queryBuilder._limit = _limit;
            queryBuilder._offset = _offset;
            queryBuilder._orderByExpressions = _orderByExpressions;

            queryBuilder._joinExpressions = _joinExpressions;
            queryBuilder._fromParams = _fromParams;
            return queryBuilder;
        }

        public QueryBuilder(DataGateORM orm)
        {
            if(!DataGateORM.ConnectionContext.Registry.Contains<TOut>())
                throw new Exception($"Type '{typeof(TOut)}' is not registered in ORM");
            _orm = orm;

            _fromParams = new HashSet<Type>();
            _fromParams.Add(typeof(TIn));
            
            _offset = null;
            _limit = null;

            _orderByExpressions = new List<Tuple<Expression, OrderByType>>();
            _joinExpressions = new List<Tuple<Expression, Type>>();
        }

        [Pure]
        public QueryBuilder<TIn, TOut> Where(Expression<Func<TIn, bool>> whereExpression)
        {
            var res = Clone();
            
            if (res._whereExpression == null)
                res._whereExpression = whereExpression.Body;
            else res._whereExpression = 
                Expression.AndAlso(res._whereExpression, whereExpression.Body);

            return res;
        }

        [Pure]
        public QueryBuilder<TSelector, TOut> Join<T, TSelector>(Expression<Func<T, TIn, bool>> onExpression)
        {
            var type = typeof(T);
            if (!DataGateORM.ConnectionContext.Registry.Contains(type))
                throw new Exception($"Type '{type}' is not registered in ORM");

            var query = ConvertInput<TSelector>();
            query._joinExpressions.Add(Tuple.Create(onExpression.Body, type));

            return query;
        }
        
        [Pure]
        public QueryBuilder<TSelector, TOut> CrossJoin<T, TSelector>()
        {
            var type = typeof(T);
            if (!DataGateORM.ConnectionContext.Registry.Contains(type))
                throw new Exception($"Type '{type}' is not registered in ORM");

            var query = ConvertInput<TSelector>();
            query._fromParams.Add(type);

            return query;
        }

        [Pure]
        public QueryBuilder<TIn, TOut> Limit(int limit)
        {
            var res = Clone();
            res._limit = limit;
            
            return res;
        }
        
        [Pure]
        public QueryBuilder<TIn, TOut> Offset(int offset)
        {
            var res = Clone();
            res._offset = offset;
            
            return res;
        }

        public QueryBuilder<TIn, TOut> Distinct()
        {
            var res = Clone();
            res._distinct = true;

            return res;
        }

        [Pure]
        public QueryBuilder<TIn, TOut> OrderBy(Expression<Func<TIn, object>> orderExpression, 
            OrderByType orderType = OrderByType.Ascending)
        {
            var res = Clone();
            res._orderByExpressions.Add(Tuple.Create(orderExpression.Body, orderType));

            return res;
        }

        [Pure]
        public QueryBuilder<TIn, TOut> OrderByDescending(Expression<Func<TIn, object>> expression)
            => OrderBy(expression, OrderByType.Descending);

        public DbContainer<TOut> ToList()
        {
            var relationship = DataGateORM.ConnectionContext.Registry.GetRelationship<TOut>();
            var command = new NpgsqlCommand();
            command.Connection = _orm.TransactionService.NpgsqlConnection;

            #region Expression parsing

            var commandText = new StringBuilder();

            var parserVisitor = new ExpressionParserVisitor(command);
            if (_whereExpression != null)
            {
                var whereText = parserVisitor.Parse(_whereExpression);
                if (!string.IsNullOrWhiteSpace(whereText))
                {
                    commandText.Append(" WHERE ");
                    commandText.Append(whereText);
                }
            }

            if (_orderByExpressions.Count > 0)
            {
                var ordersBuilder = new StringBuilder();
                var index = 0;
                foreach (var (orderExpression, orderType) in _orderByExpressions)
                {
                    var orderText = parserVisitor.Parse(orderExpression);
                    if (!string.IsNullOrWhiteSpace(orderText))
                    {
                        ordersBuilder.Append(orderText);
                        ordersBuilder.Append(orderType == OrderByType.Ascending ? " ASC" : " DESC");
                        ordersBuilder.Append(" NULLS LAST");
                        if (index != _orderByExpressions.Count - 1)
                            ordersBuilder.Append(", ");
                    }

                    index++;
                }

                var ordersText = ordersBuilder.ToString();
                if (!string.IsNullOrWhiteSpace(ordersText))
                {
                    commandText.Append(" ORDER BY ");
                    commandText.Append(ordersText);
                }
            }

            if (_limit != null && _limit.Value > 0)
                commandText.Append($" LIMIT {_limit.Value}");

            if (_offset != null && _offset.Value > 0)
                commandText.Append($" OFFSET {_offset.Value}");

            commandText.Append(";");

            var select = new StringBuilder("SELECT");
            if (_distinct)
                select.Append($" DISTINCT ON ({relationship.TableName}.datagate_id)");
            select.Append($" {relationship.TableName}.*");
            var fromText = new StringBuilder();
            fromText.Append(select);
            fromText.Append(" FROM ");

            int i = 0;
            foreach (var type in _fromParams)
            {
                var rel =
                    DataGateORM.ConnectionContext.Registry.GetRelationship(type);
                fromText.Append(rel.TableName);
                if (i != _fromParams.Count - 1)
                    fromText.Append(", ");
                i++;
            }

            i = 0;
            foreach (var (joinExpr, type) in _joinExpressions)
            {
                var rel =
                    DataGateORM.ConnectionContext.Registry.GetRelationship(type);
                fromText.Append(" JOIN ");
                fromText.Append(rel.TableName);
                fromText.Append(" ON ");
                fromText.Append(parserVisitor.Parse(joinExpr));
                if (i != _joinExpressions.Count - 1)
                    fromText.Append(", ");
                i++;
            }

            fromText.Append(commandText);
            #endregion

            command.CommandText = fromText.ToString();

            DataContext dataContext = _orm.Receiver.GetDataContext(command, relationship.TableName);
            var objects = relationship.ToDbObjects<TOut>(dataContext, _orm);

            return new DbContainer<TOut>(objects.ToList(), dataContext, _orm);
        }
    }
}