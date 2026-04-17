using CoreMod.Models;
using Share.Exceptions;

namespace UnitTest.CoreMod;

public class ModelImageInputValidatorTests
{
    [Test]
    public async Task BuildValidatedImageAttachments_ShouldAcceptValidDataUri()
    {
        var payload = Convert.ToBase64String([1, 2, 3, 4]);
        var attachments = ModelImageInputValidator.BuildValidatedImageAttachments([
            $"data:image/png;base64,{payload}"
        ]);

        await Assert.That(attachments.Count).IsEqualTo(1);
        await Assert.That(attachments[0].MediaType).IsEqualTo("image/png");
    }

    [Test]
    public async Task BuildValidatedImageAttachments_ShouldRejectTooManyImages()
    {
        var payload = Convert.ToBase64String([1, 2, 3, 4]);

        try
        {
            ModelImageInputValidator.BuildValidatedImageAttachments([
                $"data:image/png;base64,{payload}",
                $"data:image/png;base64,{payload}",
                $"data:image/png;base64,{payload}",
                $"data:image/png;base64,{payload}",
                $"data:image/png;base64,{payload}"
            ]);
            throw new InvalidOperationException("Expected BusinessException was not thrown.");
        }
        catch (BusinessException ex)
        {
            await Assert.That(ex.LanguageKey).Contains("Too many images");
        }
    }

    [Test]
    public async Task BuildValidatedImageAttachments_ShouldRejectNonImageDataUri()
    {
        var payload = Convert.ToBase64String([1, 2, 3, 4]);

        try
        {
            ModelImageInputValidator.BuildValidatedImageAttachments([
                $"data:text/plain;base64,{payload}"
            ]);
            throw new InvalidOperationException("Expected BusinessException was not thrown.");
        }
        catch (BusinessException ex)
        {
            await Assert.That(ex.LanguageKey).Contains("Only image/* data URI inputs are supported");
        }
    }
}