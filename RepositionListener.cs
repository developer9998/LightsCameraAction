using LightsCameraAction.Tools;
using UnityEngine;
using Player = GorillaLocomotion.GTPlayer;

namespace LightsCameraAction
{
    public class RepositionListener : MonoBehaviour
    {
        Repositionable moveTarget, rotateTarget;
        Transform activeHand;
        public float followRate = 40f;

        void Start()
        {
            Logging.Debug("Start");

            Plugin.Instance.gestureTracker.leftGrip.OnPressed += LeftTryMoving;
            Plugin.Instance.gestureTracker.leftTrigger.OnPressed += LeftTryRotating;
            Plugin.Instance.gestureTracker.rightGrip.OnPressed += RightTryMoving;
            Plugin.Instance.gestureTracker.rightTrigger.OnPressed += RightTryRotating;

            Plugin.Instance.gestureTracker.leftGrip.OnReleased += LeftClearMoving;
            Plugin.Instance.gestureTracker.leftTrigger.OnReleased += LeftClearRotating;
            Plugin.Instance.gestureTracker.rightGrip.OnReleased += RightClearMoving;
            Plugin.Instance.gestureTracker.rightTrigger.OnReleased += RightClearRotating;

            Logging.Debug("End");
        }

        void LeftTryMoving() { TryMoving(true); }
        void RightTryMoving() { TryMoving(false); }
        void LeftTryRotating() { TryRotating(true); }
        void RightTryRotating() { TryRotating(false); }

        void LeftClearMoving() { moveTarget = null; }
        void RightClearMoving() { moveTarget = null; }
        void LeftClearRotating() { rotateTarget = null; }
        void RightClearRotating() { rotateTarget = null; }

        void TryMoving(bool left)
        {
            var target = FindClosestRepositionable(left);
            if (!target) return;
            moveTarget = target;
            if (rotateTarget != moveTarget)
                rotateTarget = null;
            activeHand = (left ? Player.Instance.LeftHand : Player.Instance.RightHand).controllerTransform;
        }

        void TryRotating(bool left)
        {
            var target = FindClosestRepositionable(left);
            if (!target) return;
            rotateTarget = target;
            if (moveTarget != rotateTarget)
                moveTarget = null;
            activeHand = (left ? Player.Instance.LeftHand : Player.Instance.RightHand).controllerTransform;
        }

        void FixedUpdate()
        {
            if (moveTarget && moveTarget.canMove)
            {
                moveTarget.transform.position = Vector3.Lerp(
                    moveTarget.transform.position,
                    activeHand.position + Vector3.up * .1f * Player.Instance.scale,
                    followRate * Time.deltaTime
                );
            }
            if (rotateTarget && rotateTarget.canRotate)
            {
                Vector3 currentEuler = rotateTarget.transform.rotation.eulerAngles;
                Vector3 targetEuler = activeHand.rotation.eulerAngles;
                currentEuler.x = Mathf.LerpAngle(currentEuler.x, targetEuler.x, Time.deltaTime * followRate);
                currentEuler.y = Mathf.LerpAngle(currentEuler.y, targetEuler.y, Time.deltaTime * followRate);
                currentEuler.z = 0;
                rotateTarget.transform.rotation = Quaternion.Euler(currentEuler);
            }

        }

        public Repositionable FindClosestRepositionable(bool left)
        {
            Repositionable closestObject = null;
            float closestDistance = Mathf.Infinity;
            Transform playerHand = (left ? Player.Instance.LeftHand : Player.Instance.RightHand).controllerTransform;

            // Iterate through each object to find the closest one
            foreach (Repositionable repositionableObject in FindObjectsByType<Repositionable>(FindObjectsSortMode.None))
            {
                float distanceToPlayerHand = Vector3.Distance(
                    playerHand.position, 
                    repositionableObject.transform.position - repositionableObject.transform.forward * .1f * Player.Instance.scale
                );
                if (distanceToPlayerHand < closestDistance && distanceToPlayerHand < .2f * Player.Instance.scale)
                {
                    closestDistance = distanceToPlayerHand;
                    closestObject = repositionableObject;
                }
            }

            return closestObject;
        }
    }

    public class Repositionable : MonoBehaviour { public bool canRotate = true, canMove = true; }
}
