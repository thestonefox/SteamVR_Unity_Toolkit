﻿// Renderable Key Layout|Keyboard|81025
namespace VRTK
{
    using UnityEngine;

    /// <summary>
    /// A meta keyboard layout with calculated key rectangles
    /// </summary>
    /// <remarks>
    /// Renderable Key Layouts are generated by a Key Layout Calculator usually based off of a Keyboard Layout.
    /// 
    /// A RenderableKeyLayout contains:
    /// - Per-key metadata base off KeyboardLayout
    /// - Calculated positions and dimensions for each key within key area containers
    /// </remarks>
    public class VRTK_RenderableKeyLayout
    {
        /// <summary>
        /// A renderable keyset
        /// </summary>
        public class Keyset
        {
            /// <summary>
            /// The name of this keyset
            /// </summary>
            public string name;
            /// <summary>
            /// The areas that belong to this keyset
            /// </summary>
            public KeyArea[] areas;

            public override string ToString()
            {
                return VRTK_DebugHelpers.ObjectDebugString("Keyset", new string[]
                {
                    VRTK_DebugHelpers.PropertyDebugString("name", name),
                    VRTK_DebugHelpers.ArrayPropertyDebugString("areas", areas),
                });
            }
        }

        /// <summary>
        /// A renderable key area
        /// </summary>
        public class KeyArea
        {
            /// <summary>
            /// The name of this key area
            /// </summary>
            public string name;
            /// <summary>
            /// The rect this key area is placed in
            /// </summary>
            public Rect rect;
            /// <summary>
            /// The keys that belong to this key area
            /// </summary>
            public Key[] keys;
            
            public override string ToString()
            {
                return VRTK_DebugHelpers.ObjectDebugString("KeyArea", new string[]
                {
                    VRTK_DebugHelpers.PropertyDebugString("rect", rect),
                    VRTK_DebugHelpers.ArrayPropertyDebugString("keys", keys),
                });
            }
        }

        /// <summary>
        /// A renderable key
        /// </summary>
        public class Key
        {
            /// <summary>
            /// Keyboard key type
            /// </summary>
            /// <param name="Character">A key with character that should be typed.</param>
            /// <param name="KeysetModifier">A key that switches keysets.</param>
            /// <param name="Backspace">A backspace/delete key.</param>
            /// <param name="Enter">An enter/return key.</param>
            /// <param name="Done">A done key.</param>
            public enum Type
            {
                Character,
                KeysetModifier,
                Backspace,
                Enter,
                Done
            }

            /// <summary>
            /// The preferred GameObject name for this key
            /// </summary>
            public string name;
            /// <summary>
            /// The button text for this key
            /// </summary>
            public string label;
            /// <summary>
            /// The rect this key is placed in
            /// </summary>
            public Rect rect;
            /// <summary>
            /// The type of this key
            /// </summary>
            public Type type;
            /// <summary>
            /// The character to type (for type = Character)
            /// </summary>
            public char character;
            /// <summary>
            /// The keyset to switch to (for type = KeysetModifier)
            /// </summary>
            public int keyset;

            public override string ToString()
            {
                return VRTK_DebugHelpers.ObjectDebugString("Key", new string[]
                {
                    VRTK_DebugHelpers.PropertyDebugString("rect", rect),
                });
            }
        }

        /// <summary>
        /// The keysets that belong to this renderable key layout
        /// </summary>
        public Keyset[] keysets;

        public override string ToString()
        {
            return VRTK_DebugHelpers.ObjectDebugString("VRTK_RenderableKeyLayout", new string[]
            {
                VRTK_DebugHelpers.ArrayPropertyDebugString("keysets", keysets),
            });
        }
    }
}