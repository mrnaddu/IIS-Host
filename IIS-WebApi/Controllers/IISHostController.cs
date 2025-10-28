using Microsoft.AspNetCore.Mvc;

namespace IIS_WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class IISHostController(IWebHostEnvironment env) : ControllerBase
{
    private readonly IWebHostEnvironment _env = env;

    /// <summary>
    /// Get IIS server status
    /// </summary>
    /// <returns>Current server status information</returns>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        return Ok(new { Status = "Running", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Get partners and their terminals from the filesystem
    /// </summary>
    /// <returns>List of partners with their terminal folders</returns>
    [HttpGet("partners-terminals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPartnersTerminals()
    {
        var partnersRoot = Path.Combine(_env.ContentRootPath, "partners");
        if (!Directory.Exists(partnersRoot))
        {
            return Ok(Array.Empty<object>());
        }

        var partners = Directory.GetDirectories(partnersRoot)
            .Select(pDir => new
            {
                Partner = Path.GetFileName(pDir),
                Terminals = Directory.GetDirectories(pDir)
                    .Select(t => Path.GetFileName(t))
                    .OrderBy(n => n)
                    .ToArray()
            })
            .OrderBy(p => p.Partner)
            .ToArray();

        return Ok(partners);
    }

    /// <summary>
    /// Get zip file names for a specific partner and terminal
    /// </summary>
    /// <param name="partnerId">Partner folder name (e.g., partner1)</param>
    /// <param name="terminalId">Terminal folder name (e.g., terminal1)</param>
    /// <returns>List of .zip file names in the terminal folder</returns>
    [HttpGet("partners/{partnerId}/terminals/{terminalId}/zips")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTerminalZips(string partnerId, string terminalId)
    {
        if (string.IsNullOrWhiteSpace(partnerId) || string.IsNullOrWhiteSpace(terminalId))
            return NotFound();

        var terminalPath = Path.Combine(_env.ContentRootPath, "partners", partnerId, terminalId);
        if (!Directory.Exists(terminalPath))
            return NotFound();

        var zipNames = Directory.GetFiles(terminalPath, "*.zip")
            .Select(Path.GetFileName)
            .OrderBy(n => n)
            .ToArray();

        return Ok(zipNames);
    }

    /// <summary>
    /// Download a zip file for a specific partner and terminal
    /// </summary>
    /// <param name="partnerId">Partner folder name (e.g., partner1)</param>
    /// <param name="terminalId">Terminal folder name (e.g., terminal1)</param>
    /// <param name="zipName">Exact zip file name (e.g., report1.zip)</param>
    /// <returns>The zip file content</returns>
    [HttpGet("partners/{partnerId}/terminals/{terminalId}/zips/{zipName}")]
    [Produces("application/zip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DownloadZip(string partnerId, string terminalId, string zipName)
    {
        if (string.IsNullOrWhiteSpace(partnerId) || string.IsNullOrWhiteSpace(terminalId) || string.IsNullOrWhiteSpace(zipName))
            return NotFound();

        var fileName = Path.GetFileName(zipName);
        var folder = Path.Combine(_env.ContentRootPath, "partners", partnerId, terminalId);
        if (!Directory.Exists(folder)) return NotFound();

        var path = Path.Combine(folder, fileName);
        if (!System.IO.File.Exists(path))
        {
            // Fallback: find case-insensitively within the terminal folder
            var alt = Directory.EnumerateFiles(folder, "*.zip", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(p => string.Equals(Path.GetFileName(p), fileName, StringComparison.OrdinalIgnoreCase));
            if (alt is null) return NotFound();
            path = alt;
            fileName = Path.GetFileName(path);
        }

        return PhysicalFile(path, "application/zip", fileName);
    }
}
