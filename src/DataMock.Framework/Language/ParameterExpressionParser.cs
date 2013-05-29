using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LazyE9.DataMock.Language
{
    internal class ParameterExpressionParser : ExpressionVisitor
    {
        #region ParameterExpressionParser Members

        public override string ToString()
        {
            return mBuilder.ToString();
        }

        #endregion ParameterExpressionParser Members

        #region Fields

        private readonly StringBuilder mBuilder = new StringBuilder();
        private object mValue;
        private bool mExactParameter = true;

        #endregion Fields

        #region Private Members

        private void _ToStringParamIsCall(Expression expression)
        {
            Visit(expression);
        }

        private void _ToStringParamIsInRangeCall(Expression from, Expression to, Expression range)
        {
            mBuilder.Append("BETWEEN ");
            Visit(from);
            mBuilder.Append(" AND ");
            Visit(to);
        }

        private void _ToStringParamIsRegexCall(Expression expression)
        {
            mBuilder.Append("LIKE ");
            Visit(expression);
        }

        private void _ToStringParamMethodCall(string methodName, ReadOnlyCollection<Expression> parameters)
        {
            mExactParameter = false;

            switch (methodName)
            {
                case "Is":
                    _ToStringParamIsCall(parameters[0]);
                    break;
                case "IsInRange":
                    Expression from = parameters[0];
                    Expression to = parameters[1];
                    Expression range = parameters[2];
                    _ToStringParamIsInRangeCall(from, to, range);
                    break;
                case "IsRegex":
                    _ToStringParamIsRegexCall(parameters[0]);
                    break;
            }
        }

        #endregion Private Members

        #region Protected Members

        protected override Expression VisitConstant(ConstantExpression node)
        {
            mValue = node.Value;
            var valueExpression = mValue as Expression;

            if (valueExpression != null)
            {
                Visit(valueExpression);
            }
            else if (mValue != null)
            {
                if (mValue is string || mValue is Guid)
                {
                    mBuilder.Append("'").Append(mValue).Append("'");
                }
                else if (mValue is DateTime)
                {
                    mBuilder.Append("'");
                    mBuilder.Append(((DateTime)mValue).ToString("yyyy-MM-dd HH:mm:ss"));
                    mBuilder.Append("'");
                }
                else if (node.Type.IsEnum)
                {
                    mBuilder.Append((int)mValue);
                }
                else if (mValue is bool)
                {
                    mBuilder.Append((bool)mValue ? "1" : "0");
                }
                else if (mValue.ToString() == mValue.GetType().ToString())
                {
                    string message = String.Format("Expression could not be parsed: {0}", node);
                    throw new InvalidOperationException(message);
                }
                else
                {
                    mBuilder.Append(mValue);
                }
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            MethodInfo method = node.Method;
            Type declaringType = method.DeclaringType;

            if (declaringType == typeof(Param))
            {
                _ToStringParamMethodCall(method.Name, node.Arguments);
            }
            else
            {
                Visit(node);
            }
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
            {
                Visit(node.Operand);
            }
            return node;
        }

        #endregion Protected Members

        #region Internal Members

        internal static ParameterParseResult Parse(Expression expression)
        {
            expression = Evaluator.ReduceSubtrees(expression);
            var builder = new ParameterExpressionParser();
            builder.Visit(expression);
            

            return new ParameterParseResult(builder.mValue, builder.mBuilder.ToString(), builder.mExactParameter);
        }

        #endregion Internal Members

    }
}