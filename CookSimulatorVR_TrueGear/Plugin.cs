using Baking;
using Baking.GameplayFlow;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using BNG;
using HarmonyLib;
using Pizza;
using System;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using MyTrueGear;

namespace CookSimulatorVR_TrueGear
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private static int lastMicrowaveCount = 0;
        private static int lastMicrowaveMode = 0;

        private static bool lastTongsClosed = false;

        private static bool canLeftHandCutting = true;
        private static bool canRightHandCutting = true;
        private static bool canLeftHandContainerAdd = true;
        private static bool canRightHandContainerAdd = true;

        private static TrueGearMod _TrueGear = null;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));

            _TrueGear = new TrueGearMod();
            _TrueGear.Play("HeartBeat");
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static Vector3 leftHandPos = Vector3.zero;
        private static Vector3 rightHandPos = Vector3.zero;

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerVR), "Update")]
        private static void PlayerVR_Update_Postfix(PlayerVR __instance)
        {
            //Logger.LogInfo("-----------------------------------------");
            //Logger.LogInfo("Update");
            leftHandPos = __instance.LeftHand.gameObject.transform.position;
            rightHandPos = __instance.RightHand.gameObject.transform.position;
            //Logger.LogInfo($"LeftHandPos :{__instance.LeftHand.gameObject.transform.position.x},{__instance.LeftHand.gameObject.transform.position.y},{__instance.LeftHand.gameObject.transform.position.z}");
            //Logger.LogInfo($"RightHandPos :{__instance.RightHand.gameObject.transform.position.x},{__instance.RightHand.gameObject.transform.position.y},{__instance.RightHand.gameObject.transform.position.z}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GrabbableEvents), "OnGrab")]
        private static void GrabbableEvents_OnGrab_Postfix(GrabbableEvents __instance, Grabber grabber)
        {
            if (grabber.HandSide == ControllerHand.Left)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandPickupItem");
                _TrueGear.Play("LeftHandPickupItem");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandPickupItem");
                _TrueGear.Play("RightHandPickupItem");
            }
            Logger.LogInfo(grabber.HandSide);
            //Logger.LogInfo($"handPos :{grabber.gameObject.transform.position.x},{grabber.gameObject.transform.position.y},{grabber.gameObject.transform.position.z}");
            //Logger.LogInfo(__instance.grab.MainCollider.name);
            //Logger.LogInfo(__instance.grab.name);
        }


        [HarmonyPrefix, HarmonyPatch(typeof(CuttingKnifeV2), "HapticsOnSuccessCut")]
        private static void CuttingKnifeV2_HapticsOnSuccessCut_Prefix(CuttingKnifeV2 __instance, ControllerHand handSide)
        {
            if (handSide == ControllerHand.Left)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandCuttingSuccess");
                _TrueGear.Play("LeftHandCuttingSuccess");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandCuttingSuccess");
                _TrueGear.Play("RightHandCuttingSuccess");
            }
            Logger.LogInfo(handSide);
        }


        [HarmonyPostfix, HarmonyPatch(typeof(CuttingKnifeV2), "Update")]
        private static void CuttingKnifeV2_Update_Postfix(CuttingKnifeV2 __instance)
        {
            if (__instance._joints.Count < 1)
            {
                return;
            }
            foreach (CuttingKnifeV2.JointSet jointSet1 in __instance._joints)
            {
                if (!(jointSet1.joint == null) && jointSet1.size != 2)
                {
                    float num4 = (float)jointSet1.currentCheckpointIndex / ((float)(CuttingKnifeV2.numberOfCheckpoints * ((jointSet1.size == 0) ? CuttingKnifeV2.cutRateNormal : CuttingKnifeV2.cutRateSlim)) / 100f);
                    Grabber primaryGrabber1 = __instance._myGrabbable.GetPrimaryGrabber();
                    if (primaryGrabber1 && num4 > 0f)
                    {
                        if (primaryGrabber1.HandSide == ControllerHand.Left)
                        {
                            if (!canLeftHandCutting)
                            {
                                return;
                            }
                            canLeftHandCutting = false;
                            new Timer(LeftHandCuttingTimerCallBack,null,90,Timeout.Infinite);
                            Logger.LogInfo("-----------------------------------------");
                            Logger.LogInfo("LeftHandCutting");
                            _TrueGear.Play("LeftHandCutting");
                            Logger.LogInfo(primaryGrabber1.HandSide);
                            Logger.LogInfo(num4);
                        }
                        else
                        {
                            if (!canRightHandCutting)
                            {
                                return;
                            }
                            canRightHandCutting = false;
                            new Timer(RightHandCuttingTimerCallBack, null, 90, Timeout.Infinite);
                            Logger.LogInfo("-----------------------------------------");
                            Logger.LogInfo("RightHandCutting");
                            _TrueGear.Play("RightHandCutting");
                            Logger.LogInfo(primaryGrabber1.HandSide);
                            Logger.LogInfo(num4);
                        }

                    }
                }
            }
        }

        private static void LeftHandCuttingTimerCallBack(object o)
        {
            canLeftHandCutting = true;
        }
        private static void RightHandCuttingTimerCallBack(object o)
        {
            canRightHandCutting = true;
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(EggEvents), "ManageHaptics")]
        //private static void EggEvents_ManageHaptics_Prefix(EggEvents __instance, float factor)
        //{
        //    if (Time.time < __instance._lastVibrateTime + EggEvents.vibrateDuration)
        //    {
        //        return;
        //    }
        //    float factor1 = Mathf.Clamp01(factor);
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("EggManageHaptics");
        //    Logger.LogInfo(factor1);
        //}


        [HarmonyPrefix, HarmonyPatch(typeof(MovablePhysics), "OnCollisionEnter")]
        private static void MovablePhysics_OnCollisionEnter_Prefix(MovablePhysics __instance, Collision collision)
        {
            if (Explosive.justExplodedOnLowEndDevice)
            {
                return;
            }
            if (__instance.IsCollidingWithACap(collision))
            {
                return;
            }
            if (__instance.grabbable && __instance.grabbable.BeingHeld && Time.time > MovablePhysics.lastFeedbackTime + 0.25f)
            {
                if (__instance.grabbable.GetPrimaryGrabber().HandSide == ControllerHand.Left)
                {
                    Logger.LogInfo("-----------------------------------------");
                    Logger.LogInfo("LeftHandPhysicsTouch");
                    _TrueGear.Play("LeftHandPhysicsTouch");
                }
                else
                {
                    Logger.LogInfo("-----------------------------------------");
                    Logger.LogInfo("RightHandPhysicsTouch");
                    _TrueGear.Play("RightHandPhysicsTouch");
                }
                Logger.LogInfo(__instance.grabbable.GetPrimaryGrabber().HandSide);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FireSprinkler), "ActivateSprinkler")]
        private static void FireSprinkler_ActivateSprinkler_Prefix(FireSprinkler __instance)
        {
            if (__instance.particles != null)
            {
                float leftHandDis = Vector3.Distance(leftHandPos, __instance.gameObject.transform.position);
                float rightHandDis = Vector3.Distance(rightHandPos, __instance.gameObject.transform.position);
                if (leftHandDis < rightHandDis)
                {
                    Logger.LogInfo("-----------------------------------------");
                    Logger.LogInfo("StartLeftHandFireSprinkler");
                    _TrueGear.StartLeftHandFireSprinkler();
                }
                else
                {
                    Logger.LogInfo("-----------------------------------------");
                    Logger.LogInfo("StartRightHandFireSprinkler");
                    _TrueGear.StartRightHandFireSprinkler();
                }

                Logger.LogInfo($"SprinklerPos :{__instance.gameObject.transform.position.x},{__instance.gameObject.transform.position.y},{__instance.gameObject.transform.position.z}");
                Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
                Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.gameObject.transform.position)}");
                Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
                Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.gameObject.transform.position)}");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FireSprinkler), "DeactivateSprinkler")]
        private static void FireSprinkler_DeactivateSprinkler_Prefix(FireSprinkler __instance)
        {
            if (__instance.particles != null)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("StopFireSprinkler");
                _TrueGear.StopFireSprinkler();
            }
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(Container), "OnFluidLeak")]
        //private static void Container_OnFluidLeak_Prefix(Container __instance, float quantity)
        //{
        //    if (quantity > 0f)
        //    {
        //        Logger.LogInfo("-----------------------------------------");
        //        Logger.LogInfo("OnFluidLeak");
        //    }
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(CloseBottleAnimation), "OpenCap")]
        //private static void CloseBottleAnimation_OpenCap_Postfix(CloseBottleAnimation __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("OpenCap");
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(CloseBottleAnimation), "CloseCap")]
        //private static void CloseBottleAnimation_CloseCap_Postfix(CloseBottleAnimation __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("CloseCap");
        //}

        [HarmonyPostfix, HarmonyPatch(typeof(Container), "TasteContent")]
        private static void Container_TasteContent_Postfix(Container __instance)
        {
            Logger.LogInfo("-----------------------------------------");
            Logger.LogInfo("Taste");
            _TrueGear.Play("Taste");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Explosive), "Explode", new Type[] { typeof(bool), typeof(bool) })]
        private static void Explosive_Explode2_Postfix(Explosive __instance)
        {
            Logger.LogInfo("-----------------------------------------");
            Logger.LogInfo("Explosion");
            _TrueGear.Play("Explosion");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KitchenTimerEvents), "OnTriggerDown")]
        private static void KitchenTimerEvents_OnTriggerDown_Prefix(KitchenTimerEvents __instance)
        {
            if (!__instance.CollisionEnabled)
            {
                return;
            }
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandTriggerKitchenTimer");
                _TrueGear.Play("LeftHandTriggerKitchenTimer");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandTriggerKitchenTimer");
                _TrueGear.Play("RightHandTriggerKitchenTimer");
            }
            Logger.LogInfo($"SprinklerPos :{__instance.gameObject.transform.position.x},{__instance.gameObject.transform.position.y},{__instance.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.gameObject.transform.position)}");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KitchenTimer), "StartRinging")]
        private static void KitchenTimer_StartRinging_Prefix(KitchenTimer __instance)
        {
            Logger.LogInfo("-----------------------------------------");
            Logger.LogInfo("StartRinging");
            _TrueGear.Play("StartRinging");
        }


        [HarmonyPrefix, HarmonyPatch(typeof(Container), "AddVirtualProductToContainer")]
        private static void Container_AddVirtualProductToContainer_Prefix(Container __instance)
        {
            if (__instance.grabbable.GetPrimaryGrabber().HandSide == ControllerHand.Left)
            {
                if (!canLeftHandContainerAdd)
                {
                    return;
                }
                canLeftHandContainerAdd = false;
                new Timer(LeftHandContainerAddTimerCallBack, null, 90, Timeout.Infinite);
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandContainerAdd");
                _TrueGear.Play("LeftHandContainerAdd");
            }
            else if (__instance.grabbable.GetPrimaryGrabber().HandSide == ControllerHand.Right)
            {
                if (!canRightHandContainerAdd)
                {
                    return;
                }
                canRightHandContainerAdd = false;
                new Timer(RightHandContainerAddTimerCallBack, null, 90, Timeout.Infinite);
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandContainerAdd");
                _TrueGear.Play("RightHandContainerAdd");
            }
        }

        private static void LeftHandContainerAddTimerCallBack(object o)
        {
            canLeftHandContainerAdd = true;
        }
        private static void RightHandContainerAddTimerCallBack(object o)
        {
            canRightHandContainerAdd = true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SinkTap), "Sw_OnSwitchOff")]
        private static void SinkTap_Sw_OnSwitchOff_Prefix(SinkTap __instance)
        {
            if (!__instance.WaterShouldRun)
            {
                float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw1.gameObject.transform.position);
                float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw1.gameObject.transform.position);
                if (leftHandDis < rightHandDis)
                {
                    Logger.LogInfo("-----------------------------------------");
                    Logger.LogInfo("LeftHandRotaryKnob");
                    _TrueGear.Play("LeftHandRotaryKnob");
                }
                else
                {
                    Logger.LogInfo("-----------------------------------------");
                    Logger.LogInfo("RightHandRotaryKnob");
                    _TrueGear.Play("RightHandRotaryKnob");
                }
                Logger.LogInfo($"ContainerPos :{__instance.sw1.gameObject.transform.position.x},{__instance.sw1.gameObject.transform.position.y},{__instance.sw1.gameObject.transform.position.z}");
                Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
                Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw1.gameObject.transform.position)}");
                Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
                Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw1.gameObject.transform.position)}");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SinkTap), "Sw_OnSwitchOn")]
        private static void SinkTap_Sw_OnSwitchOn_Prefix(SinkTap __instance)
        {
            if (__instance.WaterShouldRun)
            {
                float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw1.gameObject.transform.position);
                float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw1.gameObject.transform.position);
                if (leftHandDis < rightHandDis)
                {
                    Logger.LogInfo("-----------------------------------------");
                    Logger.LogInfo("LeftHandRotaryKnob");
                    _TrueGear.Play("LeftHandRotaryKnob");
                }
                else
                {
                    Logger.LogInfo("-----------------------------------------");
                    Logger.LogInfo("RightHandRotaryKnob");
                    _TrueGear.Play("RightHandRotaryKnob");
                }
                Logger.LogInfo($"ContainerPos :{__instance.sw1.gameObject.transform.position.x},{__instance.sw1.gameObject.transform.position.y},{__instance.sw1.gameObject.transform.position.z}");
                Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
                Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw1.gameObject.transform.position)}");
                Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
                Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw1.gameObject.transform.position)}");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InfiniteProduct), "StartAdd")]
        private static void InfiniteProduct_StartAdd_Prefix(InfiniteProduct __instance)
        {
            try
            {
                if (__instance.AddingStart && __instance.addingCoroutine != null)
                {
                    return;
                }
                if (__instance.stats.quantity > 0f)
                {
                    if (__instance.grabbable.GetPrimaryGrabber().HandSide == ControllerHand.Left)
                    {
                        Logger.LogInfo("-----------------------------------------");
                        Logger.LogInfo("StartLeftHandAddFlavour");
                        _TrueGear.StartLeftHandAddFlavour();
                    }
                    else
                    {
                        Logger.LogInfo("-----------------------------------------");
                        Logger.LogInfo("StartRightHandAddFlavour");
                        _TrueGear.StartRightHandAddFlavour();
                    }

                }
            }
            catch(Exception e)
            {
            
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InfiniteProduct), "EndAdd")]
        private static void InfiniteProduct_EndAdd_Prefix(InfiniteProduct __instance)
        {
            if (!__instance.AddingStart)
            {
                return;
            }
            if (__instance.grabbable.GetPrimaryGrabber().HandSide == ControllerHand.Left)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("StopLeftHandAddFlavour");
                _TrueGear.StopLeftHandAddFlavour();
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("StopRightHandAddFlavour");
                _TrueGear.StopRightHandAddFlavour();
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MicrowaveTimerSnapAngle), "GetCurrentAngle")]
        private static void MicrowaveTimerSnapAngle_GetCurrentAngle_Postfix(MicrowaveTimerSnapAngle __instance, float __result)
        {
            if (!__instance.isGrabbed)
            {
                return;
            }
            int count = (int)(__result / 30);
            if (lastMicrowaveCount == count)
            {
                return;
            }
            lastMicrowaveCount = count;
            float leftHandDis = Vector3.Distance(leftHandPos, __instance._switch.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance._switch.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandRotaryParagraphKnob");
                _TrueGear.Play("LeftHandRotaryParagraphKnob");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandRotaryParagraphKnob");
                _TrueGear.Play("RightHandRotaryParagraphKnob");
            }
            Logger.LogInfo(__result);
            Logger.LogInfo(__instance.isGrabbed);
            Logger.LogInfo($"ContainerPos :{__instance._switch.gameObject.transform.position.x},{__instance._switch.gameObject.transform.position.y},{__instance._switch.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance._switch.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance._switch.gameObject.transform.position)}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MicrowaveStand), "PowerSwitch_OnSwitchMode")]
        private static void MicrowaveStand_PowerSwitch_OnSwitchMode_Postfix(MicrowaveStand __instance, int obj)
        {
            if (lastMicrowaveMode == obj)
            {
                return;
            }
            lastMicrowaveMode = obj;
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.powerSwitch.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.powerSwitch.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandRotaryParagraphKnob");
                _TrueGear.Play("LeftHandRotaryParagraphKnob");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandRotaryParagraphKnob");
                _TrueGear.Play("RightHandRotaryParagraphKnob");
            }
            Logger.LogInfo($"RotateAxis :{__instance.powerSwitch.gameObject.transform.position.x},{__instance.powerSwitch.gameObject.transform.position.y},{__instance.powerSwitch.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.powerSwitch.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.powerSwitch.gameObject.transform.position)}");


        }

        [HarmonyPostfix, HarmonyPatch(typeof(ForkEvents), "AreTongsClosed")]
        private static void ForkEvents_AreTongsClosed_Postfix(ForkEvents __instance, bool __result)
        {
            if (!__result)
            {
                lastTongsClosed = __result;
                return;
            }
            if (lastTongsClosed)
            {
                return;
            }
            lastTongsClosed = __result;
            float leftHandDis = Vector3.Distance(leftHandPos, __instance._fork.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance._fork.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandTongsClosed");
                _TrueGear.Play("LeftHandTongsClosed");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandTongsClosed");
                _TrueGear.Play("RightHandTongsClosed");
            }
            Logger.LogInfo($"ForkPos :{__instance._fork.gameObject.transform.position.x},{__instance._fork.gameObject.transform.position.y},{__instance._fork.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance._fork.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance._fork.gameObject.transform.position)}");
        }




        //[HarmonyPrefix, HarmonyPatch(typeof(Switch), "TurnOff")]
        //private static void Switch_TurnOff_Prefix(Switch __instance)
        //{
        //    if (__instance.Mode == 0)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("SwitchTurnOff");
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(Switch), "TurnOn")]
        //private static void Switch_TurnOn_Prefix(Switch __instance)
        //{
        //    if (__instance.Mode == 1)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("SwitchTurnOn");
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(Switch), "SwitchOff")]
        //private static void Switch_SwitchOff_Postfix(Switch __instance)
        //{
        //    if (__instance.mode == 0)
        //    {
        //        Logger.LogInfo("-----------------------------------------");
        //        Logger.LogInfo("SwitchSwitchOff");
        //    }
        //}






        //[HarmonyPrefix, HarmonyPatch(typeof(InfiniteProduct), "StartAddAmount",new Type[] { typeof(float), typeof(float), typeof(Action) })]
        //private static void InfiniteProduct_StartAddAmount_Prefix(InfiniteProduct __instance)
        //{
        //    if (__instance.ShouldCreateParticle)
        //    {
        //        if (__instance.stats.quantity > 0f)
        //        {
        //            Logger.LogInfo("-----------------------------------------");
        //            Logger.LogInfo("StartAddAmount");
        //        }
        //    }
        //}





        //[HarmonyPrefix, HarmonyPatch(typeof(LightSwitch), "Sw_OnSwitchOff")]
        //private static void LightSwitch_Sw_OnSwitchOff_Prefix(LightSwitch __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("LightSwitchSw_OnSwitchOff");
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(LightSwitch), "Sw_OnSwitchOn")]
        //private static void LightSwitch_Sw_OnSwitchOn_Prefix(LightSwitch __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("LightSwitchSw_OnSwitchOn");
        //}




        //[HarmonyPrefix, HarmonyPatch(typeof(KneadingMachineStand), "HandleSwitchOnClick")]
        //private static void KneadingMachineStand_HandleSwitchOnClick_Prefix(KneadingMachineStand __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("HandleSwitchOnClick");
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(KneadingMachineStand), "OnSwitchMode")]
        //private static void KneadingMachineStand_OnSwitchMode_Prefix(KneadingMachineStand __instance, float switchMode)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("OnSwitchMode");
        //    Logger.LogInfo(switchMode);
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(KneadingMachineStand), "OnSwitchOn")]
        //private static void KneadingMachineStand_OnSwitchOn_Prefix(KneadingMachineStand __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("KneadingOnSwitchOn");
        //}



        [HarmonyPostfix, HarmonyPatch(typeof(SoapDispenser), "Sw_OnSwitchOn")]      //洗洁精
        private static void SoapDispenser_Sw_OnSwitchOn_Postfix(SoapDispenser __instance)
        {
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandPressButton");
                _TrueGear.Play("LeftHandPressButton");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandPressButton");
                _TrueGear.Play("RightHandPressButton");
            }
            Logger.LogInfo($"SoapPos :{__instance.sw.gameObject.transform.position.x},{__instance.sw.gameObject.transform.position.y},{__instance.sw.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position)}");
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(SoapDispenser), "Sw_OnSwitchOff")]
        //private static void SoapDispenser_Sw_OnSwitchOff_Postfix(SoapDispenser __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("SoapDispenserSw_OnSwitchOff");
        //    Logger.LogInfo($"SoapPos :{__instance.sw.gameObject.transform.position.x},{__instance.sw.gameObject.transform.position.y},{__instance.sw.gameObject.transform.position.z}");
        //    Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
        //    Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position)}");
        //    Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
        //    Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position)}");
        //}


        [HarmonyPostfix, HarmonyPatch(typeof(HeaterSpawnPoint), "Sw_OnSwitchOn")]   //煤气灶
        private static void HeaterSpawnPoint_Sw_OnSwitchOn_Postfix(HeaterSpawnPoint __instance)
        {
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandRotaryKnob");
                _TrueGear.Play("LeftHandRotaryKnob");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandRotaryKnob");
                _TrueGear.Play("RightHandRotaryKnob");
            }
            Logger.LogInfo($"SoapPos :{__instance.sw.gameObject.transform.position.x},{__instance.sw.gameObject.transform.position.y},{__instance.sw.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position)}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HeaterSpawnPoint), "Sw_OnSwitchOff")]
        private static void HeaterSpawnPoint_Sw_OnSwitchOff_Postfix(HeaterSpawnPoint __instance)
        {
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandRotaryKnob");
                _TrueGear.Play("LeftHandRotaryKnob");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandRotaryKnob");
                _TrueGear.Play("RightHandRotaryKnob");
            }
            Logger.LogInfo($"SoapPos :{__instance.sw.gameObject.transform.position.x},{__instance.sw.gameObject.transform.position.y},{__instance.sw.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position)}");
        }


        [HarmonyPostfix, HarmonyPatch(typeof(GrillHeater), "Sw_OnSwitchOn")]        //栅格铁板
        private static void GrillHeater_Sw_OnSwitchOn_Postfix(GrillHeater __instance)
        {
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandRotaryKnob");
                _TrueGear.Play("LeftHandRotaryKnob");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandRotaryKnob");
                _TrueGear.Play("RightHandRotaryKnob");
            }
            Logger.LogInfo($"SoapPos :{__instance.sw.gameObject.transform.position.x},{__instance.sw.gameObject.transform.position.y},{__instance.sw.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position)}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GrillHeater), "Sw_OnSwitchOff")]
        private static void GrillHeater_Sw_OnSwitchOff_Postfix(GrillHeater __instance)
        {
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandRotaryKnob");
                _TrueGear.Play("LeftHandRotaryKnob");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandRotaryKnob");
                _TrueGear.Play("RightHandRotaryKnob");
            }
            Logger.LogInfo($"SoapPos :{__instance.sw.gameObject.transform.position.x},{__instance.sw.gameObject.transform.position.y},{__instance.sw.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position)}");
        }



        [HarmonyPostfix, HarmonyPatch(typeof(FrierHeater), "Set")]
        private static void FrierHeater_Set_Postfix(FrierHeater __instance, bool on)
        {
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandRotaryKnob");
                _TrueGear.Play("LeftHandRotaryKnob");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandRotaryKnob");
                _TrueGear.Play("RightHandRotaryKnob");
            }
            Logger.LogInfo($"SoapPos :{__instance.sw.gameObject.transform.position.x},{__instance.sw.gameObject.transform.position.y},{__instance.sw.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position)}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(FrierButton), "OnLeftClick")]
        private static void FrierButton_OnLeftClick_Postfix(FrierButton __instance)
        {
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandPressButton");
                _TrueGear.Play("LeftHandPressButton");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandPressButton");
                _TrueGear.Play("RightHandPressButton");
            }
            Logger.LogInfo($"SoapPos :{__instance.gameObject.transform.position.x},{__instance.gameObject.transform.position.y},{__instance.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.gameObject.transform.position)}");
        }



        //[HarmonyPostfix, HarmonyPatch(typeof(FrierHeater), "StartFillingByOil")]
        //private static void FrierHeater_StartFillingByOil_Postfix(FrierHeater __instance)
        //{
        //    float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position);
        //    float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position);
        //    if (leftHandDis < rightHandDis)
        //    {
        //        Logger.LogInfo("-----------------------------------------");
        //        Logger.LogInfo("LeftHandPressButton");
        //        _TrueGear.Play("LeftHandPressButton");
        //    }
        //    else
        //    {
        //        Logger.LogInfo("-----------------------------------------");
        //        Logger.LogInfo("RightHandPressButton");
        //        _TrueGear.Play("RightHandPressButton");
        //    }
        //    Logger.LogInfo($"SoapPos :{__instance.sw.gameObject.transform.position.x},{__instance.sw.gameObject.transform.position.y},{__instance.sw.gameObject.transform.position.z}");
        //    Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
        //    Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position)}");
        //    Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
        //    Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position)}");
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(FrierHeater), "StartDumping")]
        //private static void FrierHeater_StartDumping_Postfix(FrierHeater __instance)
        //{
        //    float leftHandDis = Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position);
        //    float rightHandDis = Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position);
        //    if (leftHandDis < rightHandDis)
        //    {
        //        Logger.LogInfo("-----------------------------------------");
        //        Logger.LogInfo("LeftHandPressButton");
        //        _TrueGear.Play("LeftHandPressButton");
        //    }
        //    else
        //    {
        //        Logger.LogInfo("-----------------------------------------");
        //        Logger.LogInfo("RightHandPressButton");
        //        _TrueGear.Play("RightHandPressButton");
        //    }
        //    Logger.LogInfo($"SoapPos :{__instance.sw.gameObject.transform.position.x},{__instance.sw.gameObject.transform.position.y},{__instance.sw.gameObject.transform.position.z}");
        //    Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
        //    Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.sw.gameObject.transform.position)}");
        //    Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
        //    Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.sw.gameObject.transform.position)}");
        //}


        //[HarmonyPostfix, HarmonyPatch(typeof(OvenHeater), "SwDoor_OnSwitchOn")]
        //private static void OvenHeater_SwDoor_OnSwitchOn_Postfix(OvenHeater __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("OvenHeaterSwDoor_OnSwitchOn");
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(OvenHeater), "SwDoor_OnSwitchOff")]
        //private static void OvenHeater_SwDoor_OnSwitchOff_Postfix(OvenHeater __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("OvenHeaterSwDoor_OnSwitchOff");
        //}


        [HarmonyPostfix, HarmonyPatch(typeof(OvenHeater), "SwMode_OnSwitchMode")]
        private static void OvenHeater_SwMode_OnSwitchMode_Postfix(OvenHeater __instance, int obj)
        {
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.swMode.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.swMode.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandRotaryKnob");
                _TrueGear.Play("LeftHandRotaryKnob");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandRotaryKnob");
                _TrueGear.Play("RightHandRotaryKnob");
            }
            Logger.LogInfo(obj);
            Logger.LogInfo($"SoapPos :{__instance.swMode.gameObject.transform.position.x},{__instance.swMode.gameObject.transform.position.y},{__instance.swMode.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.swMode.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.swMode.gameObject.transform.position)}");
        }


        //[HarmonyPostfix, HarmonyPatch(typeof(BakingKneadingMachineEvents), "NotifyKneadingMachineSwitchClicked")]
        //private static void BakingKneadingMachineEvents_NotifyKneadingMachineSwitchClicked_Postfix(BakingKneadingMachineEvents __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("NotifyKneadingMachineSwitchClicked");
        //}


        //[HarmonyPostfix, HarmonyPatch(typeof(Knob), "OnLeftClick")]
        //private static void Knob_OnLeftClick_Postfix(Knob __instance)
        //{

        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("KnobOnLeftClick");
        //}


        [HarmonyPostfix, HarmonyPatch(typeof(PushButton), "OnLeftClick")]
        private static void PushButton_OnLeftClick_Postfix(PushButton __instance)
        {
            float leftHandDis = Vector3.Distance(leftHandPos, __instance.gameObject.transform.position);
            float rightHandDis = Vector3.Distance(rightHandPos, __instance.gameObject.transform.position);
            if (leftHandDis < rightHandDis)
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("LeftHandPressButton");
                _TrueGear.Play("LeftHandPressButton");
            }
            else
            {
                Logger.LogInfo("-----------------------------------------");
                Logger.LogInfo("RightHandPressButton");
                _TrueGear.Play("RightHandPressButton");
            }
            Logger.LogInfo($"SoapPos :{__instance.gameObject.transform.position.x},{__instance.gameObject.transform.position.y},{__instance.gameObject.transform.position.z}");
            Logger.LogInfo($"LeftHandPos :{leftHandPos.x},{leftHandPos.y},{leftHandPos.z}");
            Logger.LogInfo($"LeftHandDis :{Vector3.Distance(leftHandPos, __instance.gameObject.transform.position)}");
            Logger.LogInfo($"RightHandPos :{rightHandPos.x},{rightHandPos.y},{rightHandPos.z}");
            Logger.LogInfo($"RightHandDis :{Vector3.Distance(rightHandPos, __instance.gameObject.transform.position)}");
        }









        //[HarmonyPostfix, HarmonyPatch(typeof(Lamp), "OnSwitchOn")]
        //private static void Lamp_OnSwitchOn_Postfix(Lamp __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("LampOnSwitchOn");
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(Lamp), "OnSwitchOff")]
        //private static void Lamp_OnSwitchOff_Postfix(Lamp __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("LampOnSwitchOff");
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(LightSwitch), "Sw_OnSwitchOn")]
        //private static void LightSwitch_Sw_OnSwitchOn_Postfix(LightSwitch __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("LightSwitchSw_OnSwitchOn");
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(LightSwitch), "Sw_OnSwitchOff")]
        //private static void LightSwitch_Sw_OnSwitchOff_Postfix(LightSwitch __instance)
        //{
        //    Logger.LogInfo("-----------------------------------------");
        //    Logger.LogInfo("LightSwitchSw_OnSwitchOff");
        //}


    }
}
