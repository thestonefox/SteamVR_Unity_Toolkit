﻿// Material Colour Swap|Highlighters|0020
namespace VRTK.Highlighters
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// The Material Colour Swap Highlighter is a basic implementation that simply swaps the texture colour for the given highlight colour.
    /// </summary>
    /// <remarks>
    /// Due to the way the object material is interacted with, changing the material colour will break Draw Call Batching in Unity whilst the object is highlighted.
    ///
    /// The Draw Call Batching will resume on the original material when the item is no longer highlighted.
    ///
    /// This is the default highlighter that is applied to any script that requires a highlighting component (e.g. `VRTK_Interactable_Object` or `VRTK_ControllerActions`).
    /// </remarks>
    public class VRTK_MaterialColorSwapHighlighter : VRTK_BaseHighlighter
    {
        /// <summary>
        /// The emission colour of the texture will be the highlight colour but this percent darker.
        /// </summary>
        public float emissionDarken = 50f;

        private Dictionary<string, Material[]> originalSharedRendererMaterials;
        private Dictionary<string, Material[]> originalRendererMaterials;
        private Dictionary<string, Coroutine> faderRoutines;
        private bool resetMainTexture = false;

        /// <summary>
        /// The Initialise method sets up the highlighter for use.
        /// </summary>
        /// <param name="color">Not used.</param>
        /// <param name="options">A dictionary array containing the highlighter options:\r     * `&lt;'resetMainTexture', bool&gt;` - Determines if the default main texture should be cleared on highlight. `true` to reset the main default texture, `false` to not reset it.</param>
        public override void Initialise(Color? color = null, Dictionary<string, object> options = null)
        {
            originalSharedRendererMaterials = new Dictionary<string, Material[]>();
            originalRendererMaterials = new Dictionary<string, Material[]>();
            faderRoutines = new Dictionary<string, Coroutine>();
            StoreOriginalMaterials();

            if (options != null && options.ContainsKey("resetMainTexture") && options["resetMainTexture"] != null && options["resetMainTexture"] is bool)
            {
                resetMainTexture = (bool)options["resetMainTexture"];
            }
        }

        /// <summary>
        /// The Highlight method initiates the change of colour on the object and will fade to that colour (from a base white colour) for the given duration.
        /// </summary>
        /// <param name="color">The colour to highlight to.</param>
        /// <param name="duration">The time taken to fade to the highlighted colour.</param>
        public override void Highlight(Color? color, float duration = 0f)
        {
            if (color == null)
            {
                return;
            }
            ChangeToHighlightColor((Color)color, duration);
        }

        /// <summary>
        /// The Unhighlight method returns the object back to it's original colour.
        /// </summary>
        /// <param name="color">Not used.</param>
        /// <param name="duration">Not used.</param>
        public override void Unhighlight(Color? color = null, float duration = 0f)
        {
            if (originalRendererMaterials == null)
            {
                return;
            }

            foreach (var fadeRoutine in faderRoutines)
            {
                StopCoroutine(fadeRoutine.Value);
            }
            faderRoutines.Clear();

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                var objectReference = renderer.gameObject.GetInstanceID().ToString();
                if (!originalRendererMaterials.ContainsKey(objectReference))
                {
                    continue;
                }

                renderer.materials = originalRendererMaterials[objectReference];
                renderer.sharedMaterials = originalSharedRendererMaterials[objectReference];
            }
        }

        private void StoreOriginalMaterials()
        {
            originalSharedRendererMaterials.Clear();
            originalRendererMaterials.Clear();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                var objectReference = renderer.gameObject.GetInstanceID().ToString();
                originalSharedRendererMaterials[objectReference] = renderer.sharedMaterials;
                originalRendererMaterials[objectReference] = renderer.materials;
                renderer.sharedMaterials = originalSharedRendererMaterials[objectReference];
            }
        }

        private void ChangeToHighlightColor(Color color, float duration = 0f)
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    var material = renderer.materials[i];
                    var faderRoutineID = material.GetInstanceID().ToString();

                    if (faderRoutines.ContainsKey(faderRoutineID) && faderRoutines[faderRoutineID] != null)
                    {
                        StopCoroutine(faderRoutines[faderRoutineID]);
                        faderRoutines.Remove(faderRoutineID);
                    }

                    material.EnableKeyword("_EMISSION");

                    if (resetMainTexture && material.HasProperty("_MainTex"))
                    {
                        renderer.material.SetTexture("_MainTex", new Texture());
                    }

                    if (material.HasProperty("_Color"))
                    {
                        if (duration > 0f)
                        {
                            faderRoutines[faderRoutineID] = StartCoroutine(CycleColor(material, material.color, color, duration));
                        }
                        else
                        {
                            material.color = color;
                            if (material.HasProperty("_EmissionColor"))
                            {
                                material.SetColor("_EmissionColor", Darken(color, emissionDarken));
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator CycleColor(Material material, Color startColor, Color endColor, float duration)
        {
            var elapsedTime = 0f;
            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                if (material.HasProperty("_Color"))
                {
                    material.color = Color.Lerp(startColor, endColor, (elapsedTime / duration));
                }
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", Color.Lerp(startColor, Darken(endColor, emissionDarken), (elapsedTime / duration)));
                }
                yield return null;
            }
        }

        private Color Darken(Color color, float percent)
        {
            return new Color(ColorPercent(color.r, percent), ColorPercent(color.g, percent), ColorPercent(color.b, percent), color.a);
        }

        private float ColorPercent(float value, float percent)
        {
            percent = Mathf.Clamp(percent, 0f, 100f);
            return (percent == 0f ? value : (value - (percent / 100f)));
        }
    }
}