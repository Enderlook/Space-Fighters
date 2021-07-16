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

        public static Photon.Realtime.Player GetPlayerOwner(this MonoBehaviourPun source)
            => Server.GetPlayerOwner(source.photonView);

        public static bool IsOwnerPlayer(this MonoBehaviourPun source)
            => PhotonNetwork.LocalPlayer == source.GetPlayerOwner();

        public static void RPC_ToServer(this MonoBehaviourPun source, Expression<Action> method)
        {
            (string methodName, object[] parameters) tuple = DisarmRPC(method);
            source.photonView.RPC(tuple.methodName, Server.ServerPlayer, tuple.parameters);
            ReturnArray(tuple.parameters);
        }

        public static void RPC_FromServer(this MonoBehaviourPun source, Expression<Action> method)
        {
            Debug.Assert(Server.IsServer);
            source.RPC(method, RpcTarget.All);
        }

        public static void RPC(this MonoBehaviourPun source, Expression<Action> method, RpcTarget target)
        {
            (string methodName, object[] parameters) tuple = DisarmRPC(method);
            source.photonView.RPC(tuple.methodName, target, tuple.parameters);
            ReturnArray(tuple.parameters);
        }

        private static (string methodName, object[] parameters) DisarmRPC(Expression<Action> method)
        {
            // This method is very expensive, both CPU and GC, however it allows to query RPC using lambda syntax, which looks very cool.
            // If this get too expensive, replace this with the traditional approach `RPC(nameof(Method), new object[] { p1, p2, etc });`.

            MethodCallExpression body = (MethodCallExpression)method.Body;
            Debug.Assert(body.Method.IsDefined(typeof(PunRPC)));
            object[] parameters = GetArray(body.Arguments.Count);
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                for (int i = 0; i < parameters.Length; i++)
                    parameters[i] = Interpret(body.Arguments[i]);
            }
            else
            {
                for (int i = 0; i < parameters.Length; i++)
                    parameters[i] = Expression.Lambda(body.Arguments[i]).Compile(true).DynamicInvoke();
            }
            return (body.Method.Name, parameters);
        }

        private static object Interpret(Expression expression)
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
                        parameters[i] = Interpret(call.Arguments[i]);
                    object result = call.Method.Invoke(Interpret(call.Object), parameters);
                    ReturnArray(parameters);
                    return result;
                case MemberExpression member:
                    object self = Interpret(member.Expression);
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
                default:
                    Debug.LogError($"Invalid expression type {expression.GetType()}.");
                    return null;
            }
        }

        private static object[] GetArray(int length)
        {
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
            int length = array.Length;
            Array.Clear(array, 0, length);
            pool[length].Push(array);
        }
    }
}