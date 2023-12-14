using System;

namespace YuGiOh.Scraper.Models.Exceptions;

public class InvalidBoosterPack(string message) : Exception(message);