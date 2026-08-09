using UnityEngine;

using TMPro;
using System.Collections.Generic;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown leftDropdown;
    [SerializeField] private TMP_Dropdown rightDropdown;
    [SerializeField] private List<AgentOption> agents;


    void Start(){
        // Populate both dropdowns with the available agents.
        leftDropdown.ClearOptions();
        rightDropdown.ClearOptions();

        List<string> names = new();

        foreach(var agent in agents)
            names.Add(agent.displayName);

        leftDropdown.AddOptions(names);
        rightDropdown.AddOptions(names);
    }

    public AgentOption GetLeftAgent(){
        return agents[leftDropdown.value];
    }
    public AgentOption GetRightAgent(){
        return agents[rightDropdown.value];
    }
}
