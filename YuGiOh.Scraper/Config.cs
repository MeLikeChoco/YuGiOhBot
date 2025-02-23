﻿using System;
using System.Text.Json.Serialization;

namespace YuGiOh.Scraper;

public class Config
{

    public Databases Databases { get; set; }

    [JsonPropertyName("Retry Delay")]
    public TimeSpan RetryDelay { get; set; }

    [JsonPropertyName("Max Retry")]
    public int MaxRetry { get; set; }

    [JsonPropertyName("Hash Seed")]
    public uint HashSeed { get; set; }

    public Webhook Webhook { get; set; }

}

public class Databases
{

    public string Staging { get; set; }
    public string Production { get; set; }

}

public class Webhook
{

    public string Url { get; set; }
    public string Content { get; set; }

}