//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.1
//     from Assets/Settings/InputClasses/DebugActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @DebugActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @DebugActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""DebugActions"",
    ""maps"": [
        {
            ""name"": ""DebugTools"",
            ""id"": ""2cb31f18-a3a3-47ca-9cd2-2f5a2684c6db"",
            ""actions"": [
                {
                    ""name"": ""MouseLocation"",
                    ""type"": ""Value"",
                    ""id"": ""163143e1-2f33-4c1d-9163-df86f7d3161c"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""SpawnBasic"",
                    ""type"": ""Button"",
                    ""id"": ""b3e812e2-367e-4a22-b394-1dfd3cc84412"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SpawnExploding"",
                    ""type"": ""Button"",
                    ""id"": ""7d1cf78b-9e4b-4693-99db-5a37cdecfaa1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SpawnNecro"",
                    ""type"": ""Button"",
                    ""id"": ""c0a37e79-0240-4068-96e6-919adf1d4f80"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SummonSpawnPortal"",
                    ""type"": ""Button"",
                    ""id"": ""7631f52b-c3b5-45c7-8262-1bd5c9f4428b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""19173ce2-ee0a-4adb-a4f2-e5fc1103efb0"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseLocation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6326c6b1-f395-4838-bb38-dbacd6e6f358"",
                    ""path"": ""<Keyboard>/f1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpawnBasic"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7debe059-e6e1-4503-9cf2-ed72b6a6261e"",
                    ""path"": ""<Keyboard>/f2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpawnExploding"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ff4c3ef7-1e1a-4bd4-98ee-c6328cb01bae"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SummonSpawnPortal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e9b0300f-6555-4df3-af59-99814e0024ba"",
                    ""path"": ""<Keyboard>/f3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpawnNecro"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""DebugControls"",
            ""bindingGroup"": ""DebugControls"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // DebugTools
        m_DebugTools = asset.FindActionMap("DebugTools", throwIfNotFound: true);
        m_DebugTools_MouseLocation = m_DebugTools.FindAction("MouseLocation", throwIfNotFound: true);
        m_DebugTools_SpawnBasic = m_DebugTools.FindAction("SpawnBasic", throwIfNotFound: true);
        m_DebugTools_SpawnExploding = m_DebugTools.FindAction("SpawnExploding", throwIfNotFound: true);
        m_DebugTools_SpawnNecro = m_DebugTools.FindAction("SpawnNecro", throwIfNotFound: true);
        m_DebugTools_SummonSpawnPortal = m_DebugTools.FindAction("SummonSpawnPortal", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // DebugTools
    private readonly InputActionMap m_DebugTools;
    private List<IDebugToolsActions> m_DebugToolsActionsCallbackInterfaces = new List<IDebugToolsActions>();
    private readonly InputAction m_DebugTools_MouseLocation;
    private readonly InputAction m_DebugTools_SpawnBasic;
    private readonly InputAction m_DebugTools_SpawnExploding;
    private readonly InputAction m_DebugTools_SpawnNecro;
    private readonly InputAction m_DebugTools_SummonSpawnPortal;
    public struct DebugToolsActions
    {
        private @DebugActions m_Wrapper;
        public DebugToolsActions(@DebugActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseLocation => m_Wrapper.m_DebugTools_MouseLocation;
        public InputAction @SpawnBasic => m_Wrapper.m_DebugTools_SpawnBasic;
        public InputAction @SpawnExploding => m_Wrapper.m_DebugTools_SpawnExploding;
        public InputAction @SpawnNecro => m_Wrapper.m_DebugTools_SpawnNecro;
        public InputAction @SummonSpawnPortal => m_Wrapper.m_DebugTools_SummonSpawnPortal;
        public InputActionMap Get() { return m_Wrapper.m_DebugTools; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DebugToolsActions set) { return set.Get(); }
        public void AddCallbacks(IDebugToolsActions instance)
        {
            if (instance == null || m_Wrapper.m_DebugToolsActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_DebugToolsActionsCallbackInterfaces.Add(instance);
            @MouseLocation.started += instance.OnMouseLocation;
            @MouseLocation.performed += instance.OnMouseLocation;
            @MouseLocation.canceled += instance.OnMouseLocation;
            @SpawnBasic.started += instance.OnSpawnBasic;
            @SpawnBasic.performed += instance.OnSpawnBasic;
            @SpawnBasic.canceled += instance.OnSpawnBasic;
            @SpawnExploding.started += instance.OnSpawnExploding;
            @SpawnExploding.performed += instance.OnSpawnExploding;
            @SpawnExploding.canceled += instance.OnSpawnExploding;
            @SpawnNecro.started += instance.OnSpawnNecro;
            @SpawnNecro.performed += instance.OnSpawnNecro;
            @SpawnNecro.canceled += instance.OnSpawnNecro;
            @SummonSpawnPortal.started += instance.OnSummonSpawnPortal;
            @SummonSpawnPortal.performed += instance.OnSummonSpawnPortal;
            @SummonSpawnPortal.canceled += instance.OnSummonSpawnPortal;
        }

        private void UnregisterCallbacks(IDebugToolsActions instance)
        {
            @MouseLocation.started -= instance.OnMouseLocation;
            @MouseLocation.performed -= instance.OnMouseLocation;
            @MouseLocation.canceled -= instance.OnMouseLocation;
            @SpawnBasic.started -= instance.OnSpawnBasic;
            @SpawnBasic.performed -= instance.OnSpawnBasic;
            @SpawnBasic.canceled -= instance.OnSpawnBasic;
            @SpawnExploding.started -= instance.OnSpawnExploding;
            @SpawnExploding.performed -= instance.OnSpawnExploding;
            @SpawnExploding.canceled -= instance.OnSpawnExploding;
            @SpawnNecro.started -= instance.OnSpawnNecro;
            @SpawnNecro.performed -= instance.OnSpawnNecro;
            @SpawnNecro.canceled -= instance.OnSpawnNecro;
            @SummonSpawnPortal.started -= instance.OnSummonSpawnPortal;
            @SummonSpawnPortal.performed -= instance.OnSummonSpawnPortal;
            @SummonSpawnPortal.canceled -= instance.OnSummonSpawnPortal;
        }

        public void RemoveCallbacks(IDebugToolsActions instance)
        {
            if (m_Wrapper.m_DebugToolsActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IDebugToolsActions instance)
        {
            foreach (var item in m_Wrapper.m_DebugToolsActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_DebugToolsActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public DebugToolsActions @DebugTools => new DebugToolsActions(this);
    private int m_DebugControlsSchemeIndex = -1;
    public InputControlScheme DebugControlsScheme
    {
        get
        {
            if (m_DebugControlsSchemeIndex == -1) m_DebugControlsSchemeIndex = asset.FindControlSchemeIndex("DebugControls");
            return asset.controlSchemes[m_DebugControlsSchemeIndex];
        }
    }
    public interface IDebugToolsActions
    {
        void OnMouseLocation(InputAction.CallbackContext context);
        void OnSpawnBasic(InputAction.CallbackContext context);
        void OnSpawnExploding(InputAction.CallbackContext context);
        void OnSpawnNecro(InputAction.CallbackContext context);
        void OnSummonSpawnPortal(InputAction.CallbackContext context);
    }
}
