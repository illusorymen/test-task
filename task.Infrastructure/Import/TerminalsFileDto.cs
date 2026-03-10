using System.Text.Json.Serialization;

namespace task.Infrastructure.Import;

public class TerminalsFileDto
{
    [JsonPropertyName("city")]
    public List<CityDto>? City { get; set; }
}

public class CityDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("cityID")]
    public int CityId { get; set; }

    [JsonPropertyName("terminals")]
    public TerminalsWrapperDto? Terminals { get; set; }
}

public class TerminalsWrapperDto
{
    [JsonPropertyName("terminal")]
    public List<TerminalDto>? Terminal { get; set; }
}

public class TerminalDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("fullAddress")]
    public string? FullAddress { get; set; }

    [JsonPropertyName("latitude")]
    public string? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public string? Longitude { get; set; }

    [JsonPropertyName("isPVZ")]
    public bool IsPvz { get; set; }

    [JsonPropertyName("isOffice")]
    public bool IsOffice { get; set; }

    [JsonPropertyName("phones")]
    public List<PhoneDto>? Phones { get; set; }

    [JsonPropertyName("calcSchedule")]
    public CalcScheduleDto? CalcSchedule { get; set; }

    [JsonPropertyName("worktables")]
    public WorktablesDto? Worktables { get; set; }
}

public class PhoneDto
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

public class CalcScheduleDto
{
    [JsonPropertyName("derival")]
    public string? Derival { get; set; }

    [JsonPropertyName("arrival")]
    public string? Arrival { get; set; }
}

public class WorktablesDto
{
    [JsonPropertyName("worktable")]
    public List<WorktableDto>? Worktable { get; set; }
}

public class WorktableDto
{
    [JsonPropertyName("timetable")]
    public string? Timetable { get; set; }
}
