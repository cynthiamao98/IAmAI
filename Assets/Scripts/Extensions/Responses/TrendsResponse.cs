using System;

[Serializable]
public class TrendsResponse
{
    public Trend[] trends;
    public string as_of;
    public string created_at;
}