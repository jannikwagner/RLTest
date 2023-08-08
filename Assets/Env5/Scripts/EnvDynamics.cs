using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class PlayerPosVelDynamics : IDynamicsProvider
    {
        EnvBaseAgent agent;
        public PlayerPosVelDynamics(EnvBaseAgent agent)
        {
            this.agent = agent;
        }
        public float[] dxdt(ActionBuffers action)
        {
            var velocity = agent.controller.rb.velocity;
            var acceleration = agent.GetAcceleration(action);
            var dxdt = new PosVelState { position = velocity, velocity = acceleration };
            return dxdt.ToArray();
        }
        public float[] delta_x(ActionBuffers action, float delta_t)
        {
            var dpdt = agent.controller.rb.velocity;
            var dvdt = agent.GetAcceleration(action);
            var delta_v = dvdt * delta_t;
            var delta_p = dpdt * delta_t + 0.5f * dvdt * delta_t * delta_t;
            var delta_x = new PosVelState { position = delta_p, velocity = delta_v };
            return delta_x.ToArray();
        }
        public float[] x()
        {
            var position = agent.controller.player.localPosition;
            var velocity = agent.controller.rb.velocity;
            var x = new PosVelState { position = position, velocity = velocity };
            return x.ToArray();
        }
    }
    public class PlayerTrigger1PosVelDynamics : IDynamicsProvider
    {
        // relative position and velocity of player to trigger1
        EnvBaseAgent agent;
        public PlayerTrigger1PosVelDynamics(EnvBaseAgent agent)
        {
            this.agent = agent;
        }
        public float[] dxdt(ActionBuffers action)
        {
            var trigger1Velocity = agent.controller.env.trigger1.GetComponentInParent<Rigidbody>().velocity;
            var velocity = agent.controller.rb.velocity - trigger1Velocity;
            var acceleration = agent.GetAcceleration(action);
            var dxdt = new PosVelState { position = velocity, velocity = acceleration };
            return dxdt.ToArray();
        }
        public float[] delta_x(ActionBuffers action, float delta_t)
        {
            var trigger1Velocity = agent.controller.env.trigger1.GetComponentInParent<Rigidbody>().velocity;
            var dpdt = agent.controller.rb.velocity - trigger1Velocity;
            var dvdt = agent.GetAcceleration(action);
            var delta_v = dvdt * delta_t;
            var delta_p = dpdt * delta_t + 0.5f * dvdt * delta_t * delta_t;
            var delta_x = new PosVelState { position = delta_p, velocity = delta_v };
            return delta_x.ToArray();
        }

        public float[] x()
        {
            var position = agent.controller.player.localPosition - agent.controller.env.trigger1.localPosition;
            var velocity = agent.controller.rb.velocity - agent.controller.env.trigger1.GetComponentInParent<Rigidbody>().velocity;
            var x = new PosVelState { position = position, velocity = velocity };
            return x.ToArray();
        }
    }
}