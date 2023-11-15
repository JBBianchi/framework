﻿using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neuroglia.Measurements;

/// <summary>
/// Enumerates all types of units of measure
/// </summary>
[TypeConverter(typeof(EnumMemberTypeConverter))]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnitOfMeasurementType
{
    /// <summary>
    /// Indicates a unit used to measure capacity
    /// </summary>
    [EnumMember(Value = "capacity")]
    Capacity = 1,
    /// <summary>
    /// Indicates a unit used to measure energy
    /// </summary>
    [EnumMember(Value = "energy")]
    Energy = 2,
    /// <summary>
    /// Indicates a unit used to measure a distance between two points
    /// </summary>
    [EnumMember(Value = "length")]
    Length = 4,
    /// <summary>
    /// Indicates a unit used to measure weight
    /// </summary>
    [EnumMember(Value = "mass")]
    Mass = 8,
    /// <summary>
    /// Indicates a unit used to measure surfaces
    /// </summary>
    [EnumMember(Value = "surface")]
    Surface = 16,
    /// <summary>
    /// Indicates a unit used to measure unfractable units
    /// </summary>
    [EnumMember(Value = "unit")]
    Unit = 32,
    /// <summary>
    /// Indicates a unit used to measure volumes
    /// </summary>
    [EnumMember(Value = "volume")]
    Volume = 64
}