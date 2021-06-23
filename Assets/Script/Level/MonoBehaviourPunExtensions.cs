using Photon.Pun;

using System;
using System.Linq.Expressions;

using UnityEngine;

namespace Game.Level
{
    public static class MonoBehaviourPunExtensions
    {
        public static Photon.Realtime.Player GetPlayerOwner(this MonoBehaviourPun source)
            => Server.GetPlayerOwner(source.photonView);

        public static bool IsOwnerPlayer(this MonoBehaviourPun source)
            => PhotonNetwork.LocalPlayer == source.GetPlayerOwner();

        public static void RPC_ToServer(this MonoBehaviourPun source, Expression<Action> method)
        {
            (string methodName, object[] parameters) tuple = DisarmRPC(method);
            source.photonView.RPC(tuple.methodName, Server.ServerPlayer, tuple.parameters);
        }

        public static void RPC_FromServer(this MonoBehaviourPun source, Expression<Action> method)
        {
            Debug.Assert(Server.IsServer);
            (string methodName, object[] parameters) tuple = DisarmRPC(method);
            source.photonView.RPC(tuple.methodName, RpcTarget.All, tuple.parameters);
        }

        private static (string methodName, object[] parameters) DisarmRPC(Expression<Action> method)
        {
            // This method is very expensive, both CPU and GC, however it allows to query RPC using lambda syntax, which looks very cool.
            // If this get too expensive, replace this with the traditional approach `RPC(nameof(Method), new object[] { p1, p2, etc });`.

            MethodCallExpression body = (MethodCallExpression)method.Body;
            object[] parameters = new object[body.Arguments.Count];
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
            if (expression is null)
                return null;
            if (expression is ConstantExpression constant)
                return constant.Value;
            MethodCallExpression call = (MethodCallExpression)expression;
            object[] parameters = new object[call.Arguments.Count];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = Interpret(call.Arguments[i]);
            return call.Method.Invoke(Interpret(call.Object), parameters);
        }
    }
}