using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LazyE9.DataMock.Language
{
    internal class ExpressionStringBuilder
    {
        #region ExpressionStringBuilder Members

        public override string ToString()
        {
            return mBuilder.ToString();
        }

        #endregion ExpressionStringBuilder Members

        #region Fields

        private readonly StringBuilder mBuilder = new StringBuilder();

        #endregion Fields

        #region Private Members

        private void _AsCommaSeparatedValues<T>(IEnumerable<T> source, Action<T> toStringAction) where T : Expression
        {
            bool appendComma = false;
            foreach (T exp in source)
            {
                if (appendComma)
                {
                    mBuilder.Append(", ");
                }
                toStringAction(exp);
                appendComma = true;
            }
        }

        private static bool _NeedEncloseInParen(Expression operand)
        {
            return operand.NodeType == ExpressionType.AndAlso || operand.NodeType == ExpressionType.OrElse;
        }

        private void _ToStringBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.ArrayIndex)
            {
                Append(b.Left);
                mBuilder.Append("[");
                Append(b.Right);
                mBuilder.Append("]");
            }
            else
            {
                string @operator = ToStringOperator(b.NodeType);
                if (_NeedEncloseInParen(b.Left))
                {
                    mBuilder.Append("(");
                    Append(b.Left);
                    mBuilder.Append(")");
                }
                else
                {
                    Append(b.Left);
                }
                mBuilder.Append(" ");
                mBuilder.Append(@operator);
                mBuilder.Append(" ");
                if (_NeedEncloseInParen(b.Right))
                {
                    mBuilder.Append("(");
                    Append(b.Right);
                    mBuilder.Append(")");
                }
                else
                {
                    Append(b.Right);
                }
            }
        }

        private void _ToStringBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    _ToStringMemberAssignment((MemberAssignment)binding);
                    return;
                case MemberBindingType.MemberBinding:
                    _ToStringMemberMemberBinding((MemberMemberBinding)binding);
                    return;
                case MemberBindingType.ListBinding:
                    _ToStringMemberListBinding((MemberListBinding)binding);
                    return;
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        private void _ToStringBindingList(IEnumerable<MemberBinding> original)
        {
            bool appendComma = false;
            foreach (MemberBinding exp in original)
            {
                if (appendComma)
                {
                    mBuilder.Append(", ");
                }
                _ToStringBinding(exp);
                appendComma = true;
            }
        }

        private void _ToStringConditional(ConditionalExpression c)
        {
            Append(c.Test);
            Append(c.IfTrue);
            Append(c.IfFalse);
        }

        private void _ToStringConstant(ConstantExpression c)
        {
            object value = c.Value;
            var valueExpression = value as Expression;

            if (valueExpression != null)
            {
                Append(valueExpression);
            }
            else if (value != null)
            {
                if (value is string || value is Guid)
                {
                    mBuilder.Append("'").Append(value).Append("'");
                }
                else if (value is DateTime)
                {
                    mBuilder.Append("'");
                    mBuilder.Append(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"));
                    mBuilder.Append("'");
                }
                else if (value.ToString() == value.GetType().ToString())
                {
                    string message = String.Format("Expression could not be parsed: {0}", c);
                    throw new InvalidOperationException(message);
                }
                else if (c.Type.IsEnum)
                {
                    mBuilder.Append((int)value);
                }
                else
                {
                    mBuilder.Append(value);
                }
            }
            else
            {
                mBuilder.Append("null");
            }
        }

        private void _ToStringElementInitializer(ElementInit initializer)
        {
            mBuilder.Append("{ ");
            _ToStringExpressionList(initializer.Arguments);
            mBuilder.Append(" }");
        }

        private void _ToStringElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            for (int i = 0, n = original.Count; i < n; i++)
            {
                _ToStringElementInitializer(original[i]);
            }
        }

        private void _ToStringExpressionList(IEnumerable<Expression> original)
        {
            _AsCommaSeparatedValues(original, Append);
        }

        private void _ToStringInvocation(InvocationExpression iv)
        {
            _ToStringExpressionList(iv.Arguments);
        }

        private void _ToStringLambda(LambdaExpression lambda)
        {
            if (lambda.Parameters.Count == 1)
            {
                _ToStringParameter(lambda.Parameters[0]);
            }
            else
            {
                mBuilder.Append("(");
                _AsCommaSeparatedValues(lambda.Parameters, _ToStringParameter);
                mBuilder.Append(")");
            }
            mBuilder.Append(" => ");
            Append(lambda.Body);
        }

        private void _ToStringListInit(ListInitExpression init)
        {
            _ToStringNew(init.NewExpression);
            mBuilder.Append(" { ");
            bool appendComma = false;
            foreach (ElementInit initializer in init.Initializers)
            {
                if (appendComma)
                {
                    mBuilder.Append(", ");
                }
                _ToStringElementInitializer(initializer);
                appendComma = true;
            }
            mBuilder.Append(" }");
        }

        private void _ToStringMemberAccess(MemberExpression m)
        {
            if (m.Expression != null)
            {
                Append(m.Expression);
            }
            mBuilder.Append(".");
            mBuilder.Append(m.Member.Name);
        }

        private void _ToStringMemberAssignment(MemberAssignment assignment)
        {
            mBuilder.Append(assignment.Member.Name);
            mBuilder.Append("= ");
            Append(assignment.Expression);
        }

        private void _ToStringMemberInit(MemberInitExpression init)
        {
            _ToStringNew(init.NewExpression);
            mBuilder.Append(" { ");
            _ToStringBindingList(init.Bindings);
            mBuilder.Append(" }");
        }

        private void _ToStringMemberListBinding(MemberListBinding binding)
        {
            _ToStringElementInitializerList(binding.Initializers);
        }

        private void _ToStringMemberMemberBinding(MemberMemberBinding binding)
        {
            _ToStringBindingList(binding.Bindings);
        }

        private void _ToStringMethodCall(MethodCallExpression node)
        {
            if (node != null)
            {
                int paramFrom = 0;
                Expression expression = node.Object;


                MethodInfo method = node.Method;
                Type declaringType = method.DeclaringType;

                if (declaringType == typeof(Param))
                {
                    _ToStringParamMethodCall(method.Name, node.Arguments);
                }
                else
                {
                    if (Attribute.GetCustomAttribute(method, typeof(ExtensionAttribute)) != null)
                    {
                        paramFrom = 1;
                        expression = node.Arguments[0];
                    }

                    if (expression != null)
                    {
                        Append(expression);
                    }


                    if (method.IsPropertyIndexerGetter())
                    {
                        mBuilder.Append("[");
                        _AsCommaSeparatedValues(node.Arguments.Skip(paramFrom), Append);
                        mBuilder.Append("]");
                    }
                    else if (method.IsPropertyIndexerSetter())
                    {
                        mBuilder.Append("[");
                        _AsCommaSeparatedValues(node.Arguments
                                                    .Skip(paramFrom)
                                                    .Take(node.Arguments.Count - paramFrom), Append);
                        mBuilder.Append("] = ");
                        Append(node.Arguments.Last());
                    }
                    else if (method.IsPropertyGetter())
                    {
                        mBuilder.Append(".").Append(method.Name.Substring(4));
                    }
                    else if (method.IsPropertySetter())
                    {
                        mBuilder.Append(".").Append(method.Name.Substring(4)).Append(" = ");
                        Append(node.Arguments.Last());
                    }
                    else
                    {
                        mBuilder
                            .Append(".")
                            .Append(method.Name)
                            .Append("(");
                        _AsCommaSeparatedValues(node.Arguments.Skip(paramFrom), Append);
                        mBuilder.Append(")");
                    }
                }
            }
        }

        private void _ToStringNew(NewExpression nex)
        {
            mBuilder.Append("new ");

            mBuilder.Append("(");
            _AsCommaSeparatedValues(nex.Arguments, Append);
            mBuilder.Append(")");
        }

        private void _ToStringNewArray(NewArrayExpression na)
        {
            switch (na.NodeType)
            {
                case ExpressionType.NewArrayInit:
                    mBuilder.Append("new[] { ");
                    _AsCommaSeparatedValues(na.Expressions, Append);
                    mBuilder.Append(" }");
                    return;
                case ExpressionType.NewArrayBounds:
                    mBuilder.Append("new ");
                    mBuilder.Append("[");
                    _AsCommaSeparatedValues(na.Expressions, Append);
                    mBuilder.Append("]");
                    return;
            }
        }

        private void _ToStringParameter(ParameterExpression p)
        {
            mBuilder.Append(p.Name);
        }

        private void _ToStringParamIsCall(Expression expression)
        {
            Append(expression);
        }

        private void _ToStringParamIsInRangeCall(Expression from, Expression to, Expression range)
        {
            mBuilder.Append("BETWEEN ");
            Append(from);
            mBuilder.Append(" AND ");
            Append(to);
        }

        private void _ToStringParamIsRegexCall(Expression expression)
        {
            mBuilder.Append("LIKE ");
            Append(expression);
        }

        private void _ToStringParamMethodCall(string methodName, ReadOnlyCollection<Expression> parameters)
        {
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

        private void _ToStringTypeIs(TypeBinaryExpression b)
        {
            Append(b.Expression);
        }

        private void _ToStringUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    Append(u.Operand);
                    return;

                case ExpressionType.ArrayLength:
                    Append(u.Operand);
                    mBuilder.Append(".Length");
                    return;

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    mBuilder.Append("-");
                    Append(u.Operand);
                    return;

                case ExpressionType.Not:
                    mBuilder.Append("!(");
                    Append(u.Operand);
                    mBuilder.Append(")");
                    return;

                case ExpressionType.Quote:
                    Append(u.Operand);
                    return;

                case ExpressionType.TypeAs:
                    mBuilder.Append("(");
                    Append(u.Operand);
                    mBuilder.Append(" as ");
                    mBuilder.Append(")");
                    return;
            }
        }

        #endregion Private Members

        #region Internal Members

        internal void Append(Expression exp)
        {
            if (exp == null)
            {
                mBuilder.Append("null");
                return;
            }
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    _ToStringUnary((UnaryExpression)exp);
                    return;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    _ToStringBinary((BinaryExpression)exp);
                    return;
                case ExpressionType.TypeIs:
                    _ToStringTypeIs((TypeBinaryExpression)exp);
                    return;
                case ExpressionType.Conditional:
                    _ToStringConditional((ConditionalExpression)exp);
                    return;
                case ExpressionType.Constant:
                    _ToStringConstant((ConstantExpression)exp);
                    return;
                case ExpressionType.Parameter:
                    _ToStringParameter((ParameterExpression)exp);
                    return;
                case ExpressionType.MemberAccess:
                    _ToStringMemberAccess((MemberExpression)exp);
                    return;
                case ExpressionType.Call:
                    _ToStringMethodCall((MethodCallExpression)exp);
                    return;
                case ExpressionType.Lambda:
                    _ToStringLambda((LambdaExpression)exp);
                    return;
                case ExpressionType.New:
                    _ToStringNew((NewExpression)exp);
                    return;
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    _ToStringNewArray((NewArrayExpression)exp);
                    return;
                case ExpressionType.Invoke:
                    _ToStringInvocation((InvocationExpression)exp);
                    return;
                case ExpressionType.MemberInit:
                    _ToStringMemberInit((MemberInitExpression)exp);
                    return;
                case ExpressionType.ListInit:
                    _ToStringListInit((ListInitExpression)exp);
                    return;
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        internal static string GetString(Expression expression)
        {
            expression = Evaluator.ReduceSubtrees(expression);
            var builder = new ExpressionStringBuilder();
            builder.Append(expression);
            return builder.ToString();
        }

        internal static string ToStringOperator(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";

                case ExpressionType.And:
                    return "&";

                case ExpressionType.AndAlso:
                    return "&&";

                case ExpressionType.Coalesce:
                    return "??";

                case ExpressionType.Divide:
                    return "/";

                case ExpressionType.Equal:
                    return "==";

                case ExpressionType.ExclusiveOr:
                    return "^";

                case ExpressionType.GreaterThan:
                    return ">";

                case ExpressionType.GreaterThanOrEqual:
                    return ">=";

                case ExpressionType.LeftShift:
                    return "<<";

                case ExpressionType.LessThan:
                    return "<";

                case ExpressionType.LessThanOrEqual:
                    return "<=";

                case ExpressionType.Modulo:
                    return "%";

                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";

                case ExpressionType.NotEqual:
                    return "!=";

                case ExpressionType.Or:
                    return "|";

                case ExpressionType.OrElse:
                    return "||";

                case ExpressionType.Power:
                    return "^";

                case ExpressionType.RightShift:
                    return ">>";

                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
            }
            return nodeType.ToString();
        }

        #endregion Internal Members

    }
}