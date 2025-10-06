using UnityEngine;
using UnityEngine.UI;

public class RotatingDisc : MonoBehaviour
{
    public enum State
    {
        SpeedUp,
        Rotating,
        SpeedDown,
        Settle,
        Shoot,
        Wait,
    }
    public Transform rotor;
    public Image[] images;
    public Image pointer;
    public Text text;
    public int value = 1;
    private float displayValue;
    public int playerId;

    public float SHOOT_VALUE = 1f / 10;
    private State state = State.SpeedUp;
    private float rotateAngle;
    private float rotateSpeed;
    private float duration;
    private void Start()
    {
        pointer.color = GameMgr.Instance.battle.players[playerId].color;
        GameMgr.Instance.OnRestart += Instance_OnRestart;
        Instance_OnRestart();
    }

    private void Instance_OnRestart()
    {
        displayValue = value = 1;
        state = State.SpeedUp;
        rotateAngle = 0;
        rotateSpeed = 0;
        rotor.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        UpdateValue();
        gameObject.SetActive(true);
    }
    private void OnDestroy()
    {
        GameMgr.Instance.OnRestart -= Instance_OnRestart;
    }
    private (float, float, float, float) GetValues(float value)
    {
        if (value > 100_000) return (1, 0, 0, 0);

        var mulValue = Mathf.Sqrt(1f / value);
        var addValue = Mathf.Pow(mulValue, .85f);
        var logValue = Mathf.Pow(mulValue, 1.15f);
        var total = SHOOT_VALUE + addValue + mulValue + logValue;
        var shootValue = SHOOT_VALUE / total;
        addValue /= total;
        mulValue /= total;
        logValue /= total;
        return (shootValue, addValue, mulValue, logValue);
    }
    private void UpdateImages()
    {
        (var shootValue, var addValue, var mulValue, var logValue) = GetValues(displayValue);
        images[0].fillAmount = shootValue;
        images[1].fillAmount = addValue;
        images[2].fillAmount = mulValue;
        images[3].fillAmount = logValue;
        var total = 0f;
        foreach (var image in images)
        {
            image.transform.localRotation = Quaternion.AngleAxis(-360 * total, Vector3.forward);
            total += image.fillAmount;
        }
    }

    private void Update()
    {
        var player = GameMgr.Instance.battle.players[playerId];
        switch (state)
        {
            case State.SpeedUp:
                if (rotateSpeed < 4)
                    rotateSpeed += Time.deltaTime * 3;
                else
                {
                    state = State.Rotating;
                    duration = Random.Range(0f, 2);
                }
                break;
            case State.Rotating:
                if (duration > 0)
                    duration -= Time.deltaTime;
                else
                    state = State.SpeedDown;
                break;
            case State.SpeedDown:
                if (rotateSpeed > 0)
                    rotateSpeed -= Time.deltaTime;
                else
                {
                    state = State.Settle;
                    rotateSpeed = 0;
                }
                break;
            case State.Settle:
                {
                    var angleValue = rotateAngle / (Mathf.PI * 2);
                    (var shootValue, var addValue, var mulValue, _) = GetValues(displayValue);

                    if (angleValue < shootValue)
                        state = State.Shoot;
                    else
                    {
                        angleValue -= shootValue;
                        if (angleValue < addValue)
                        {
                            value += 100;
                        }
                        else
                        {
                            angleValue -= addValue;
                            if (angleValue < mulValue)
                            {
                                value *= 2;
                            }
                            else
                            {
                                value = (int)(value * Mathf.Log(Mathf.Max(value, 2), 2));
                            }
                        }
                        duration = .5f;
                        state = State.Wait;
                    }
                }
                break;
            case State.Shoot:
                {
                    for (int i = 0; i < 25 && value > 1 && player.cores.Count > 0; i++, value--)
                    {
                        GameMgr.Instance.battle.AddEnegry(playerId);
                    }
                    if (value == 1)
                    {
                        duration = .5f;
                        state = State.Wait;
                    }
                }
                break;
            case State.Wait:
            default:
                if (duration > 0)
                    duration -= Time.deltaTime;
                else
                    state = State.SpeedUp;
                break;
        }
        if (rotateSpeed > 0)
        {
            rotateAngle += Mathf.PI * 2 * Time.deltaTime * rotateSpeed;
            if (rotateAngle > Mathf.PI * 2) rotateAngle -= Mathf.PI * 2;
            rotor.rotation = Quaternion.Euler(0, 0, rotateAngle * Mathf.Rad2Deg);
        }

        if (value != displayValue) UpdateValue();

        if (player.soldier == 0 && player.cores.Count == 0)
            gameObject.SetActive(false);
    }
    private void UpdateValue()
    {
        displayValue = Mathf.Lerp(value, displayValue, .95f);
        UpdateImages();
        text.text = FormatNumber(Mathf.RoundToInt(displayValue));
    }
    public static string FormatNumber(long num)
    {
        var s = num.ToString();
        if (s.Length <= 3) return s;
        var unitIndex = (s.Length - 1) / 3;
        var part = s[..3];
        if (s.Length % 3 == 0) return part + units[unitIndex];
        return part.Insert(s.Length % 3, ".") + units[unitIndex];
    }
    public static readonly string[] units = new string[] { "", "K", "M", "B", "T", "P", "E", "Z", "Y", "N", "D" };
}
