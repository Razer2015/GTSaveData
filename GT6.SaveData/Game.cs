using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace GT.SaveData;

public enum Game
{
    GTHD,
    GTPSP,
    GT5P,
    GT5TTC,
    GT5,
    GT6GC,
    GT6
}

public static class GameMapper
{

    private static readonly Dictionary<string, Game> TitleIdToGameMap = new()
    {
        // Gran Turismo 5
        { "BCUS98272", Game.GT5 }, // Collector's Edition, Physical, PS3, America
        { "BCUS98114", Game.GT5 }, // Original, Physical, PS3, North America
        { "BCUS99267", Game.GT5 }, // Original, Physical, PS3, Canada
        { "BCAS20164", Game.GT5 }, // Original, Physical, PS3, Asia
        { "BCUS98114SA", Game.GT5 }, // Original, Physical, PS3, Brazil
        { "BCAS20108", Game.GT5 }, // Original, Physical, PS3, Asia
        { "BCKS10096", Game.GT5 }, // Original, Physical, PS3, Korea
        { "BCJS30001", Game.GT5 }, // First Print Limited Edition, Physical, PS3, Japan
        { "BCAS20154", Game.GT5 }, // Original, Physical, PS3, Asia
        { "BCAS20151", Game.GT5 }, // First Print Limited Edition, Physical, PS3, Asia
        { "BCES00569", Game.GT5 }, // Press Kit, Physical, PS3, Europe
        { "BCJB95009", Game.GT5 }, // Bundled, Physical, PS3, Japan
        { "BCJS30050", Game.GT5 }, // Original, Physical, PS3, Japan
        { "BCAS20267", Game.GT5 }, // 2013 Edition, Original, Physical, PS3, Asia
        { "BCJS30100", Game.GT5 }, // Spec II, Original, Physical, PS3, Japan
        { "BCUS90696", Game.GT5 }, // XL Edition, Bundled, Physical, PS3, Brazil
        { "BCUS98394", Game.GT5 }, // XL Edition, Original, Bundled, Physical, PS3, America, Brazil, North America
        // Gran Turismo HD
        { "NPEA90002", Game.GTHD },
        // Gran Turismo 5 Prologue
        { "NPJA90061", Game.GT5P },
        { "BCJS30017", Game.GT5P },
        // Gran Turismo 5 Time Trial Challenge
        { "NPHA80080", Game.GT5TTC },
        { "NPUA70087", Game.GT5TTC },
        { "NPEA90052", Game.GT5TTC },
        // Gran Turismo 6
        { "BCJS37016", Game.GT6 },
        // Gran Turismo 6 GC
        { "DEMO32768", Game.GT6GC },
    };

    public static Game? GetGameByTitleId(string titleId) =>
        TitleIdToGameMap.TryGetValue(titleId, out Game game) ? game : null;

}
