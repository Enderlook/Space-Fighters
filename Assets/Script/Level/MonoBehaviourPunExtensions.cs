using Photon.Pun;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using UnityEngine;

namespace Game.Level
{
    public static class MonoBehaviourPunExtensions
    {
        private static Dictionary<int, Stack<object[]>> pool = new Dictionary<int, Stack<object[]>>();
        private static object[] empty = new object[0];

        public static Photon.Realtime.Player GetPlayerOwner(this MonoBehaviourPun source)
            => Server.GetPlayerOwner(source.photonView);

        public static bool IsOwnerPlayer(this MonoBehaviourPun source)
            => PhotonNetwork.LocalPlayer == source.GetPlayerOwner();

        public static void RPC_ToServer(this MonoBehaviourPun source, Expression<Action> lambda)
        {
            (string methodName, object[] parameters) tuple = DisarmRPC(lambda, empty);
            source.photonView.RPC(tuple.methodName, Server.ServerPlayer, tuple.parameters);
            ReturnArray(tuple.parameters);
        }

        public static void RPC_FromServer(this MonoBehaviourPun source, Expression<Action> lambda)
        {
            Debug.Assert(Server.IsServer);
            source.RPC(lambda, RpcTarget.All);
        }

        public static void RPC_FromServer<T>(this MonoBehaviourPun source, Expression<Action<T>> lambda, T parameter)
        {
            Debug.Assert(Server.IsServer);
            source.RPC(lambda, parameter, RpcTarget.All);
        }

        public static void RPC_FromServer<T1, T2>(this MonoBehaviourPun source, Expression<Action<T1, T2>> lambda, T1 parameter1, T2 parameter2)
        {
            Debug.Assert(Server.IsServer);
            source.RPC(lambda, parameter1, parameter2, RpcTarget.All);
        }

        public static void RPC_FromServer<T1, T2, T3>(this MonoBehaviourPun source, Expression<Action<T1, T2, T3>> lambda, T1 parameter1, T2 parameter2, T3 parameter3)
        {
            Debug.Assert(Server.IsServer);
            source.RPC(lambda, parameter1, parameter2, parameter3, RpcTarget.All);
        }

        public static void RPC(this MonoBehaviourPun source, Expression<Action> method, RpcTarget target)
            => source.RPC(method, empty, target);

        public static void RPC<T>(this MonoBehaviourPun source, Expression<Action<T>> method, T parameter, RpcTarget target)
        {
            object[] parameters = GetArray(1);
            parameters[0] = parameter;
            source.RPC(method, parameters, target);
            ReturnArray(parameters);
        }

        public static void RPC<T1, T2>(this MonoBehaviourPun source, Expression<Action<T1, T2>> method, T1 parameter1, T2 parameter2, RpcTarget target)
        {
            object[] parameters = GetArray(2);
            parameters[0] = parameter1;
            parameters[1] = parameter2;
            source.RPC(method, parameters, target);
            ReturnArray(parameters);
        }

        public static void RPC<T1, T2, T3>(this MonoBehaviourPun source, Expression<Action<T1, T2, T3>> method, T1 parameter1, T2 parameter2, T3 parameter3, RpcTarget target)
        {
            object[] parameters = GetArray(3);
            parameters[0] = parameter1;
            parameters[1] = parameter2;
            parameters[2] = parameter3;
            source.RPC(method, parameters, target);
            ReturnArray(parameters);
        }

        private static void RPC(this MonoBehaviourPun source, LambdaExpression lambda, object[] parameters, RpcTarget target)
        {
            (string methodName, object[] parameters) tuple = DisarmRPC(lambda, parameters);
            source.photonView.RPC(tuple.methodName, target, tuple.parameters);
            ReturnArray(tuple.parameters);
        }

        private static (string methodName, object[] parameters) DisarmRPC(LambdaExpression lambda, object[] parameters)
        {
            // This method is very expensive, both CPU and GC, however it allows to query RPC using lambda syntax, which looks very cool.
            // If this get too expensive, replace this with the traditional approach `RPC(nameof(Method), new object[] { p1, p2, etc });`.

            MethodCallExpression body = (MethodCallExpression)lambda.Body;
            Debug.Assert(body.Method.IsDefined(typeof(PunRPC)));
            Debug.Assert(lambda.Parameters.Count == parameters.Length);
            object[] parameters_ = GetArray(body.Arguments.Count);
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                for (int i = 0; i < parameters_.Length; i++)
                    parameters_[i] = Interpret(body.Arguments[i], lambda, parameters);
            }
            else
            {
                for (int i = 0; i < parameters_.Length; i++)
                {
                    Expression argument = body.Arguments[i];
                    if (argument is ParameterExpression parameter)
                    {
                        for (int j = 0; j < lambda.Parameters.Count; j++)
                        {
                            if (lambda.Parameters[j] == parameter)
                            {
                                parameters_[i] = parameters[j];
                                break;
                            }
                        }
                    }
                    else
                        parameters_[i] = Expression.Lambda(argument).Compile(true).DynamicInvoke();
                }
            }
            return (body.Method.Name, parameters_);
        }

        private static object Interpret(Expression expression, LambdaExpression lambda, object[] methodParameters)
        {
            switch (expression)
            {
                case null:
                    return null;
                case ConstantExpression constant:
                    return constant.Value;
                case MethodCallExpression call:
                    object[] parameters = GetArray(call.Arguments.Count);
                    for (int i = 0; i < parameters.Length; i++)
                        parameters[i] = Interpret(call.Arguments[i], lambda, methodParameters);
                    object result = call.Method.Invoke(Interpret(call.Object, lambda, methodParameters), parameters);
                    ReturnArray(parameters);
                    return result;
                case MemberExpression member:
                    object self = Interpret(member.Expression, lambda, methodParameters);
                    switch (member.Member)
                    {
                        case PropertyInfo property:
                            return property.GetValue(self);
                        case FieldInfo field:
                            return field.GetValue(self);
                        default:
                            Debug.LogError("Invalid member type.");
                            return null;
                    }
                case ParameterExpression parameter:
                    for (int i = 0; i < lambda.Parameters.Count; i++)
                        if (lambda.Parameters[i] == parameter)
                            return methodParameters[i];
                    throw new ArgumentException("Parameter not found.");
                default:
                    Debug.LogError($"Invalid expression type {expression.GetType()}.");
                    return null;
            }
        }

        private static object[] GetArray(int length)
        {
            if (length == 0)
                return empty;
            if (pool.TryGetValue(length, out Stack<object[]> stack))
            {
                if (stack.TryPop(out object[] array))
                    return array;
                goto allocate;
            }
            pool.Add(length, new Stack<object[]>());
            allocate:
            return new object[length];
        }

        private static void ReturnArray(object[] array)
        {
            if (array == empty)
                return;

            int length = array.Length;
            Array.Clear(array, 0, length);
            pool[length].Push(array);
        }
    }
}