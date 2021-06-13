using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertnessMeter : MonoBehaviour
{
    Slider awarenessSlider;

    Entity_Enemy boss;

    GameObject player;

    float alertnessRate = .5f;
    float alertnessTotal = 0;
    float alertnessMax = 1;
    float distanceEqualizer = 5;

    void Awake()
    {
        awarenessSlider = GetComponent<Slider>();
        boss = GetComponentInParent<Entity_Enemy>();
        player = FindObjectOfType<Entity_Player>().gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        awarenessSlider.maxValue = alertnessMax;
    }

    // Update is called once per frame
    void Update()
    {

        awarenessSlider.value = alertnessTotal;
        if (alertnessTotal > alertnessMax && boss.CheckLOS(player) == true && (boss.curState == Entity_Enemy.EnemyStates.Search || boss.curState == Entity_Enemy.EnemyStates.Pursue))
        {
            boss.EnterPursue(player);
        }
    }

    public void IncreaseAwareness(float distance)
    {
        float alertnessIncrease = alertnessRate * Time.deltaTime * (distanceEqualizer / distance);
        alertnessTotal += alertnessIncrease;
        //Debug.Log("New Alertness total is " + alertnessTotal);
    }

    public void LowerAwareness()
    {
        if (alertnessTotal > 0)
        {
            alertnessTotal -= 1 * Time.deltaTime;
            //Debug.Log("New Alertness total is " + alertnessTotal);
        }
    }

    public void ResetAwareness()
    {
        alertnessTotal = 0;
    }
}
