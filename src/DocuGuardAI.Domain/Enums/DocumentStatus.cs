namespace DocuGuardAI.Domain.Enums;

public enum DocumentStatus
{
    Uploaded = 0,                 // just received
    TextExtracted = 1,            // OCR done
    SafetyCheckPassed = 2,        // Content Safety OK → can continue
    Unprocessed = 3,              // Content Safety failed (or other hard failure)
    Processing = 4,               // further analysis running
    Completed = 5,
    Failed = 6
}