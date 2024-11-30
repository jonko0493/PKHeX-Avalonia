using System;
using PKHeX.Core;
using SkiaSharp;
using static PKHeX.Core.MoveType;

namespace PKHeX.Drawing.PokeSprite;

/// <summary>
/// Utility class for getting the color of a <see cref="MoveType"/>.
/// </summary>
public static class TypeColor
{
    /// <summary>
    /// Gets the color of a <see cref="MoveType"/>.
    /// </summary>
    /// <param name="type">Type to get the color of.</param>
    /// <returns>Color of the type.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static SKColor GetTypeSpriteColor(byte type) => ((MoveType)type).GetTypeSpriteColor();

    public static SKColor GetTypeSpriteColor(this MoveType type) => type switch
    {
        Normal   => new(159, 161, 159),
        Fighting => new(255, 128, 000),
        Flying   => new(129, 185, 239),
        Poison   => new(143, 065, 203),
        Ground   => new(145, 081, 033),
        Rock     => new(175, 169, 129),
        Bug      => new(145, 161, 025),
        Ghost    => new(112, 065, 112),
        Steel    => new(096, 161, 184),
        Fire     => new(230, 040, 041),
        Water    => new(041, 128, 239),
        Grass    => new(063, 161, 041),
        Electric => new(250, 192, 000),
        Psychic  => new(239, 065, 121),
        Ice      => new(063, 216, 255),
        Dragon   => new(080, 097, 225),
        Dark     => new(080, 065, 063),
        Fairy    => new(239, 113, 239),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    /// <summary>
    /// Color to show for a <see cref="MoveType"/> of <see cref="TeraTypeUtil.Stellar"/>.
    /// </summary>
    public static SKColor Stellar => SKColors.LightYellow;

    /// <summary>
    /// Gets the color of a <see cref="MoveType"/> for a Tera sprite.
    /// </summary>
    public static SKColor GetTeraSpriteColor(byte elementalType)
    {
        if (elementalType == TeraTypeUtil.Stellar)
            return Stellar;
        return GetTypeSpriteColor(elementalType);
    }
}
