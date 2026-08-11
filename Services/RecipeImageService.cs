/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: RecipeImageService.cs
 *
 * Purpose:
 * Provides the validation and storage helpers used by the
 * recipe forms so a recipe image can either be uploaded from
 * the user's computer or supplied as an external image URL.
 *
 * Responsibilities:
 *   • Validate externally hosted image URLs
 *   • Validate uploaded files (type and size)
 *   • Store uploaded files under wwwroot/uploads/recipes
 *     using a generated, safe file name
 *   • Resolve a stored value into a safe image source that
 *     the display components can render
 * ===========================================================
 */

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;

namespace ChefConnect.Services;

/// <summary>
/// Helper methods for recipe images.
///
/// A recipe image is stored in a single Recipe.ImageUrl column.
/// The column holds either an absolute http/https URL or a
/// relative path such as "/uploads/recipes/abc123.jpg".
/// </summary>
public static class RecipeImageService
{
    /// <summary>
    /// Largest upload accepted by the application (5 MB).
    /// </summary>
    public const long MaxImageSizeBytes = 5L * 1024 * 1024;

    /// <summary>
    /// Value used for the file input's "accept" attribute.
    /// </summary>
    public const string AcceptedFileTypes = ".jpg,.jpeg,.png,.webp";

    /// <summary>
    /// Public request path of the uploads folder.
    /// </summary>
    public const string UploadsRequestPath = "/uploads/recipes";

    // Folder names are declared as constants so the physical
    // path is never built from user supplied text.
    private const string UploadsRootFolder = "uploads";
    private const string UploadsRecipesFolder = "recipes";

    // Optional folder used instead of wwwroot when the application
    // is hosted. It is set once at startup and never changes.
    private static string? configuredStorageDirectory;

    /// <summary>
    /// File extensions accepted for uploaded recipe images.
    /// </summary>
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    /// <summary>
    /// Content types accepted for uploaded recipe images.
    /// </summary>
    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/pjpeg",
            "image/png",
            "image/webp"
        };

    /// <summary>
    /// Matches the relative paths produced by
    /// <see cref="SaveUploadedImageAsync"/>. Only generated file
    /// names are accepted, which keeps directory traversal
    /// sequences out of the stored value.
    /// </summary>
    private static readonly Regex UploadedImagePattern =
        new(@"^/uploads/recipes/[A-Za-z0-9]{1,64}\.(jpg|jpeg|png|webp)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // =========================================================
    // Validation
    // =========================================================

    /// <summary>
    /// Determines whether an externally hosted image URL is
    /// usable. An empty value is allowed because the image is
    /// optional.
    /// </summary>
    /// <param name="imageUrl">The URL entered by the user.</param>
    public static bool IsValidExternalImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return true;
        }

        return Uri.TryCreate(imageUrl.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp
                || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Determines whether a value already stored on a recipe is
    /// a locally uploaded image.
    /// </summary>
    /// <param name="imageUrl">The stored recipe image value.</param>
    public static bool IsUploadedImagePath(string? imageUrl)
    {
        return !string.IsNullOrWhiteSpace(imageUrl)
            && UploadedImagePattern.IsMatch(imageUrl.Trim());
    }

    /// <summary>
    /// Determines whether an uploaded file is an accepted image.
    /// </summary>
    /// <param name="file">The file chosen by the user.</param>
    public static bool IsAllowedImageFile(IBrowserFile? file)
    {
        if (file is null)
        {
            return false;
        }

        var extension = Path.GetExtension(file.Name);

        var hasAllowedExtension =
            !string.IsNullOrWhiteSpace(extension)
            && AllowedExtensions.Contains(extension);

        var hasAllowedContentType =
            AllowedContentTypes.Contains(file.ContentType ?? string.Empty);

        return hasAllowedExtension && hasAllowedContentType;
    }

    /// <summary>
    /// Validates a chosen file and returns the message that should
    /// be displayed to the user.
    /// </summary>
    /// <param name="file">The file chosen by the user.</param>
    /// <returns>
    /// Null when the file is acceptable; otherwise the reason the
    /// file was rejected.
    /// </returns>
    public static string? GetFileValidationMessage(IBrowserFile? file)
    {
        if (file is null)
        {
            return null;
        }

        if (!IsAllowedImageFile(file))
        {
            return "Only JPG, JPEG, PNG, and WEBP images can be uploaded.";
        }

        if (file.Size > MaxImageSizeBytes)
        {
            return "The image must be 5 MB or smaller.";
        }

        return null;
    }

    // =========================================================
    // Display
    // =========================================================

    /// <summary>
    /// Converts a stored recipe image value into a source that is
    /// safe to place in an img tag.
    ///
    /// Existing external URLs and locally uploaded paths are both
    /// supported. Anything else is treated as "no image" so the
    /// placeholder is displayed instead.
    /// </summary>
    /// <param name="imageUrl">The stored recipe image value.</param>
    public static string? ResolveImageSource(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var value = imageUrl.Trim();

        if (IsUploadedImagePath(value))
        {
            return value;
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp
                || uri.Scheme == Uri.UriSchemeHttps))
        {
            return uri.AbsoluteUri;
        }

        return null;
    }

    // =========================================================
    // Storage
    // =========================================================

    /// <summary>
    /// Sets the folder that uploaded images are written to.
    ///
    /// This is called once at startup. When the application is
    /// hosted, the folder is outside the deployed application so
    /// that images are not lost each time the site is redeployed.
    /// An empty value keeps the default wwwroot location, which is
    /// what happens when running locally.
    /// </summary>
    /// <param name="absolutePath">
    /// Absolute path of the folder holding recipe images.
    /// </param>
    public static void UseStorageDirectory(string? absolutePath)
    {
        configuredStorageDirectory = string.IsNullOrWhiteSpace(absolutePath)
            ? null
            : absolutePath.Trim();
    }

    /// <summary>
    /// Returns the physical uploads folder and creates it when it
    /// does not exist yet.
    /// </summary>
    /// <param name="environment">The web hosting environment.</param>
    public static string EnsureUploadsDirectory(IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        string uploadsDirectory;

        if (configuredStorageDirectory is not null)
        {
            uploadsDirectory = configuredStorageDirectory;
        }
        else
        {
            var webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
                ? Path.Combine(environment.ContentRootPath, "wwwroot")
                : environment.WebRootPath;

            uploadsDirectory = Path.Combine(
                webRootPath,
                UploadsRootFolder,
                UploadsRecipesFolder);
        }

        Directory.CreateDirectory(uploadsDirectory);

        return uploadsDirectory;
    }

    /// <summary>
    /// Saves an uploaded image to wwwroot/uploads/recipes using a
    /// generated file name and returns the relative path that
    /// should be stored on the recipe.
    /// </summary>
    /// <param name="environment">The web hosting environment.</param>
    /// <param name="file">The file chosen by the user.</param>
    /// <returns>
    /// A relative path such as "/uploads/recipes/abc123.jpg".
    /// </returns>
    public static async Task<string> SaveUploadedImageAsync(
        IWebHostEnvironment environment,
        IBrowserFile file)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(file);

        // Never write a file that failed validation.
        var validationMessage = GetFileValidationMessage(file);

        if (validationMessage is not null)
        {
            throw new InvalidOperationException(validationMessage);
        }

        var uploadsDirectory = EnsureUploadsDirectory(environment);

        // The original file name is never used. Only the extension
        // is kept, and it has already been checked against the
        // allowed list above.
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();

        var generatedFileName = $"{Guid.NewGuid():N}{extension}";

        var destinationPath = Path.Combine(
            uploadsDirectory,
            generatedFileName);

        await using (var source = file.OpenReadStream(MaxImageSizeBytes))
        await using (var destination = File.Create(destinationPath))
        {
            await source.CopyToAsync(destination);
        }

        return $"{UploadsRequestPath}/{generatedFileName}";
    }

    // =========================================================
    // Preview
    // =========================================================

    /// <summary>
    /// Produces a small preview of the chosen file so the user can
    /// see the image before the recipe is saved.
    ///
    /// The browser resizes the image first, which keeps the preview
    /// small enough to send over the Blazor Server connection.
    /// </summary>
    /// <param name="file">The file chosen by the user.</param>
    /// <returns>
    /// A data URL for the preview, or null when a preview cannot
    /// be produced.
    /// </returns>
    public static async Task<string?> CreatePreviewAsync(IBrowserFile? file)
    {
        if (file is null || !IsAllowedImageFile(file))
        {
            return null;
        }

        try
        {
            // Resizing happens in the browser, so only the smaller
            // preview image travels to the server.
            var previewFormat = file.ContentType.Equals(
                "image/webp",
                StringComparison.OrdinalIgnoreCase)
                    ? "image/webp"
                    : "image/jpeg";

            var resized = await file.RequestImageFileAsync(
                previewFormat,
                600,
                600);

            using var buffer = new MemoryStream();

            await using (var stream = resized.OpenReadStream(MaxImageSizeBytes))
            {
                await stream.CopyToAsync(buffer);
            }

            var base64 = Convert.ToBase64String(buffer.ToArray());

            return $"data:{previewFormat};base64,{base64}";
        }
        catch (Exception)
        {
            // A preview is a convenience only. If the browser cannot
            // produce one the upload itself still works.
            return null;
        }
    }
}
