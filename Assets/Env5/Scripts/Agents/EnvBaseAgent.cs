using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Env5
{
    public class EnvBaseAgent : BaseAgent
    {
        public override void CollectObservations(VectorSensor sensor)  // currently not used but overridden
        {
            Vector3 playerPos = controller.player.localPosition;
            Vector3 playerPosObs = playerPos / controller.env.Width * 2f;
            sensor.AddObservation(playerPosObs);
            sensor.AddObservation(controller.rb.velocity / controller.maxSpeed);
            Vector3 targetPos = controller.env.target.localPosition;
            Vector3 distanceToTargetObs = (targetPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToTargetObs);
            Vector3 goalTriggerPos = controller.env.goalTrigger.localPosition;
            Vector3 distanceToGoalTriggerObs = (goalTriggerPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToGoalTriggerObs);
            Vector3 buttonPos = controller.env.button.localPosition;
            Vector3 distanceToButtonObs = (buttonPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToButtonObs);
            Vector3 goalPos = controller.env.goal.localPosition;
            Vector3 distanceToGoalObs = (goalPos - playerPos) / controller.env.Width;
            sensor.AddObservation(distanceToGoalObs);
        }

        public PlayerController controller;
        private IEnvActuator actuator;
        public override int NumActions => actuator.NumActions;

        void Start()
        {
            actuator = new EnvActuator25();
        }
        public Vector3 GetAcc(ActionBuffers actions)
        {
            return actuator.GetAcc(actions);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var acc = GetAcc(actions);
            controller.ApplyAcc(acc);
            base.OnActionReceived(actions);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            actuator.Heuristic(actionsOut);
        }

        public override void ResetEnvLocal()
        {
            this.controller.env.Initialize();
        }
        public override void ResetEnvGlobal()
        {
            this.controller.env.Initialize();
        }

        protected override void OnACCViolation()
        {
            // Debug.Log("OnACCViolation" + controller.player.localPosition + controller.env.PlayerUp() + controller.env.DistancePlayerUp());
        }
    }

    public interface IEnvActuator
    {
        int NumActions { get; }
        Vector3 GetAcc(ActionBuffers actions);
        void Heuristic(in ActionBuffers actionsOut);
    }

    public class EnvActuator25 : IEnvActuator
    {
        public int NumActions => 25;
        public Vector3 GetAcc(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            var i = action % 5;
            var j = action / 5;
            var acc = new Vector3(i - 2, 0f, j - 2) / 2.0f;
            return acc;
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            int factor = Input.GetKey(KeyCode.Space) ? 2 : 1;
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = factor * (int)Input.GetAxisRaw("Horizontal") + 2;
            var j = factor * (int)Input.GetAxisRaw("Vertical") + 2;
            discreateActionsOut[0] = i + 5 * j;
            // Debug.Log(discreateActionsOut[0]);
        }
    }
    public class EnvActuator9 : IEnvActuator
    {
        public int NumActions => 9;

        public Vector3 GetAcc(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            var i = action % 3;
            var j = action / 3;
            var force = new Vector3(i - 1, 0f, j - 1);
            return force;
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = (int)Input.GetAxisRaw("Horizontal") + 1;
            var j = (int)Input.GetAxisRaw("Vertical") + 1;
            discreateActionsOut[0] = i + 3 * j;
            // Debug.Log(discreateActionsOut[0]);
        }
    }
    public class EnvActuator5 : IEnvActuator
    {
        public int NumActions => 5;

        public Vector3 GetAcc(ActionBuffers actions)
        {
            var discreteActions = actions.DiscreteActions;
            var action = discreteActions[0];

            switch (action)
            {
                case 1:
                    return new Vector3(0f, 0f, 1f);
                case 2:
                    return new Vector3(0f, 0f, -1f);
                case 3:
                    return new Vector3(1f, 0f, 0f);
                case 4:
                    return new Vector3(-1f, 0f, 0f);
                case 0:
                default:
                    return new Vector3(0f, 0f, 0f);

            }
        }

        public void Heuristic(in ActionBuffers actionsOut)
        {
            var discreateActionsOut = actionsOut.DiscreteActions;

            var i = (int)Input.GetAxisRaw("Horizontal");
            var j = (int)Input.GetAxisRaw("Vertical");
            if (i == 0 && j == 0)
            {
                discreateActionsOut[0] = 0;
            }
            else if (i == 0)
            {
                discreateActionsOut[0] = j > 0 ? 1 : 2;
            }
            else
            {
                discreateActionsOut[0] = i > 0 ? 3 : 4;
            }
            // Debug.Log(discreateActionsOut[0]);
        }
    }
    public class PosVelDynamics : IDynamicsProvider
    {
        EnvBaseAgent agent;
        public PosVelDynamics(EnvBaseAgent agent)
        {
            this.agent = agent;
        }
        public float[] dxdt(ActionBuffers action)
        {
            var velocity = agent.controller.rb.velocity;
            var acc = agent.GetAcc(action);
            var dxdt = new PosVelState { position = velocity, velocity = acc };
            return dxdt.ToArray();
        }

        public float[] x()
        {
            var position = agent.controller.player.localPosition;
            var velocity = agent.controller.rb.velocity;
            var x = new PosVelState { position = position, velocity = velocity };
            return x.ToArray();
        }
    }
}
