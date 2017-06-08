﻿// Straight Pointer Renderer|PointerRenderers|10020
namespace VRTK
{
    using UnityEngine;

    /// <summary>
    /// The Straight Pointer Renderer emits a coloured beam from the end of the object it is attached to and simulates a laser beam.
    /// </summary>
    /// <remarks>
    /// It can be useful for pointing to objects within a scene and it can also determine the object it is pointing at and the distance the object is from the controller the beam is being emitted from.
    /// </remarks>
    /// <example>
    /// `VRTK/Examples/003_Controller_SimplePointer` shows the simple pointer in action and code examples of how the events are utilised and listened to can be viewed in the script `VRTK/Examples/Resources/Scripts/VRTK_ControllerPointerEvents_ListenerExample.cs`
    /// </example>
    [AddComponentMenu("VRTK/Scripts/Pointers/Pointer Renderers/VRTK_StraightPointerRenderer")]
    public class VRTK_StraightPointerRenderer : VRTK_BasePointerRenderer
    {
        [Header("Straight Pointer Appearance Settings")]

        [Tooltip("The maximum length the pointer tracer can reach.")]
        public float maximumLength = 100f;
        [Tooltip("The scale factor to scale the pointer tracer object by.")]
        public float scaleFactor = 0.002f;
        [Tooltip("The scale multiplier to scale the pointer cursor object by in relation to the `Scale Factor`.")]
        public float cursorScaleMultiplier = 25f;
        [Tooltip("The cursor will be rotated to match the angle of the target surface if this is true, if it is false then the pointer cursor will always be horizontal.")]
        public bool cursorMatchTargetRotation = false;
        [Tooltip("Rescale the cursor proportionally to the distance from the tracer origin.")]
        public bool cursorDistanceRescale = false;
        [Tooltip("The maximum scale the cursor is allowed to reach. This is only used when rescaling the cursor proportionally to the distance from the tracer origin.")]
        public Vector3 maximumCursorScale = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

        [Header("Straight Pointer Custom Appearance Settings")]

        [Tooltip("A custom game object to use as the appearance for the pointer tracer. If this is empty then a Box primitive will be created and used.")]
        public GameObject customTracer;
        [Tooltip("A custom game object to use as the appearance for the pointer cursor. If this is empty then a Sphere primitive will be created and used.")]
        public GameObject customCursor;

        protected GameObject actualContainer;
        protected GameObject actualTracer;
        protected GameObject actualCursor;

        protected Vector3 cursorOriginalScale = Vector3.one;

        /// <summary>
        /// The UpdateRenderer method is used to run an Update routine on the pointer.
        /// </summary>
        public override void UpdateRenderer()
        {
            if ((controllingPointer && controllingPointer.IsPointerActive()) || IsVisible())
            {
                float tracerLength = CastRayForward();
                SetPointerAppearance(tracerLength);
                MakeRenderersVisible();
            }
            base.UpdateRenderer();
        }

        /// <summary>
        /// The GetPointerObjects returns an array of the auto generated GameObjects associated with the pointer.
        /// </summary>
        /// <returns>An array of pointer auto generated GameObjects.</returns>
        public override GameObject[] GetPointerObjects()
        {
            return new GameObject[] { actualContainer, actualCursor, actualTracer };
        }

        protected override void ToggleRenderer(bool pointerState, bool actualState)
        {
            ToggleElement(actualTracer, pointerState, actualState, tracerVisibility, ref tracerVisible);
            ToggleElement(actualCursor, pointerState, actualState, cursorVisibility, ref cursorVisible);
        }

        protected override void CreatePointerObjects()
        {
            actualContainer = new GameObject(VRTK_SharedMethods.GenerateVRTKObjectName(true, gameObject.name, "StraightPointerRenderer_Container"));
            actualContainer.transform.localPosition = Vector3.zero;
            VRTK_PlayerObject.SetPlayerObject(actualContainer, VRTK_PlayerObject.ObjectTypes.Pointer);

            CreateTracer();
            CreateCursor();
            Toggle(false, false);
            if (controllingPointer)
            {
                controllingPointer.ResetActivationTimer(true);
                controllingPointer.ResetSelectionTimer(true);
            }
        }

        protected override void DestroyPointerObjects()
        {
            if (actualContainer != null)
            {
                Destroy(actualContainer);
            }
        }

        protected override void ChangeMaterial(Color givenColor)
        {
            base.ChangeMaterial(givenColor);
            ChangeMaterialColor(actualTracer, givenColor);
            ChangeMaterialColor(actualCursor, givenColor);
        }

        protected override void UpdateObjectInteractor()
        {
            base.UpdateObjectInteractor();
            //if the object interactor is too far from the pointer tip then set it to the pointer tip position to prevent glitching.
            if (objectInteractor && actualCursor && Vector3.Distance(objectInteractor.transform.position, actualCursor.transform.position) > 0f)
            {
                objectInteractor.transform.position = actualCursor.transform.position;
            }
        }

        protected virtual void CreateTracer()
        {
            if (customTracer)
            {
                actualTracer = Instantiate(customTracer);
            }
            else
            {
                actualTracer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                actualTracer.GetComponent<BoxCollider>().isTrigger = true;
                actualTracer.AddComponent<Rigidbody>().isKinematic = true;
                actualTracer.layer = LayerMask.NameToLayer("Ignore Raycast");

                SetupMaterialRenderer(actualTracer);
            }

            actualTracer.transform.name = VRTK_SharedMethods.GenerateVRTKObjectName(true, gameObject.name, "StraightPointerRenderer_Tracer");
            actualTracer.transform.SetParent(actualContainer.transform);

            VRTK_PlayerObject.SetPlayerObject(actualTracer, VRTK_PlayerObject.ObjectTypes.Pointer);
        }

        protected virtual void CreateCursor()
        {
            if (customCursor)
            {
                actualCursor = Instantiate(customCursor);
            }
            else
            {
                actualCursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                actualCursor.transform.localScale = Vector3.one * (scaleFactor * cursorScaleMultiplier);
                actualCursor.GetComponent<Collider>().isTrigger = true;
                actualCursor.AddComponent<Rigidbody>().isKinematic = true;
                actualCursor.layer = LayerMask.NameToLayer("Ignore Raycast");

                SetupMaterialRenderer(actualCursor);
            }

            cursorOriginalScale = actualCursor.transform.localScale;
            actualCursor.transform.name = VRTK_SharedMethods.GenerateVRTKObjectName(true, gameObject.name, "StraightPointerRenderer_Cursor");
            actualCursor.transform.SetParent(actualContainer.transform);
            VRTK_PlayerObject.SetPlayerObject(actualCursor, VRTK_PlayerObject.ObjectTypes.Pointer);
        }

        protected virtual void CheckRayMiss(bool rayHit, RaycastHit pointerCollidedWith)
        {
            if (!rayHit || (destinationHit.collider && destinationHit.collider != pointerCollidedWith.collider))
            {
                if (destinationHit.collider != null)
                {
                    PointerExit(destinationHit);
                }

                destinationHit = new RaycastHit();
                ChangeColor(invalidCollisionColor);
            }
        }

        protected virtual void CheckRayHit(bool rayHit, RaycastHit pointerCollidedWith)
        {
            if (rayHit)
            {
                if (pointerCollidedWith.collider && pointerCollidedWith.collider != destinationHit.collider)
                {
                    PointerEnter(pointerCollidedWith);
                }
                destinationHit = pointerCollidedWith;
                ChangeColor(validCollisionColor);
            }
        }

        protected virtual float CastRayForward()
        {
            Transform origin = GetOrigin();
            Ray pointerRaycast = new Ray(origin.position, origin.forward);
            RaycastHit pointerCollidedWith;
#pragma warning disable 0618
            bool rayHit = VRTK_CustomRaycast.Raycast(customRaycast, pointerRaycast, out pointerCollidedWith, layersToIgnore, maximumLength);
#pragma warning restore 0618

            CheckRayMiss(rayHit, pointerCollidedWith);
            CheckRayHit(rayHit, pointerCollidedWith);

            float actualLength = maximumLength;
            if (rayHit && pointerCollidedWith.distance < maximumLength)
            {
                actualLength = pointerCollidedWith.distance;
            }

            return OverrideBeamLength(actualLength);
        }

        protected virtual void SetPointerAppearance(float tracerLength)
        {
            if (actualContainer)
            {
                //if the additional decimal isn't added then the beam position glitches
                float beamPosition = tracerLength / (2f + BEAM_ADJUST_OFFSET);

                actualTracer.transform.localScale = new Vector3(scaleFactor, scaleFactor, tracerLength);
                actualTracer.transform.localPosition = Vector3.forward * beamPosition;
                actualCursor.transform.localScale = Vector3.one * (scaleFactor * cursorScaleMultiplier);
                actualCursor.transform.localPosition = new Vector3(0f, 0f, tracerLength);

                Transform origin = GetOrigin();
                actualContainer.transform.position = origin.position;
                actualContainer.transform.rotation = origin.rotation;

                ScaleObjectInteractor(actualCursor.transform.localScale * 1.05f);

                if (destinationHit.transform)
                {
                    if (cursorMatchTargetRotation)
                    {
                        actualCursor.transform.forward = -destinationHit.normal;
                    }
                    if (cursorDistanceRescale)
                    {
                        float collisionDistance = Vector3.Distance(destinationHit.point, origin.position);
                        actualCursor.transform.localScale = Vector3.Min(cursorOriginalScale * collisionDistance, maximumCursorScale);
                    }
                }
                else
                {
                    if (cursorMatchTargetRotation)
                    {
                        actualCursor.transform.forward = origin.forward;
                    }
                    if (cursorDistanceRescale)
                    {
                        actualCursor.transform.localScale = Vector3.Min(cursorOriginalScale * tracerLength, maximumCursorScale);
                    }
                }

                ToggleRenderer(controllingPointer.IsPointerActive(), false);
                UpdateDependencies(actualCursor.transform.position);
            }
        }
    }
}