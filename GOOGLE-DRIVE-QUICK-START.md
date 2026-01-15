# Google Drive Setup - Quick Reference

## 🚀 Quick Start (5 phút)

### 1. Lấy Service Account JSON
1. Google Cloud Console → Create Service Account
2. Enable Google Drive API
3. Download JSON key file

### 2. Setup Environment Variable

**PowerShell (Windows):**
```powershell
$json = Get-Content "service-account-key.json" -Raw
$env:GOOGLE_DRIVE_CREDENTIALS = $json
```

**Bash (Linux/Mac):**
```bash
export GOOGLE_DRIVE_CREDENTIALS=$(cat service-account-key.json)
```

### 3. Tạo Folder Trên Drive
1. Tạo root folder: `GreenDragonTrading`
2. Share với service account email (từ JSON file)
3. Copy folder ID từ URL

### 4. Config
```json
{
  "GoogleDrive": {
    "RootFolderId": "YOUR_FOLDER_ID",
    "AutoCreateFolders": true,
    "Enabled": true
  }
}
```

### 5. Run!
```bash
dotnet run --project GreenDragonTrading.Api
```

## 📁 Folder Types Available

```csharp
DriveFolderConstants.FINANCIAL_REPORTS
DriveFolderConstants.COMPANY_LOGOS
DriveFolderConstants.USER_DOCUMENTS
DriveFolderConstants.EXPORTS
DriveFolderConstants.ATTACHMENTS
DriveFolderConstants.IMAGES
DriveFolderConstants.TEMPLATES
DriveFolderConstants.BACKUPS
```

## 🔧 Usage

```csharp
// Upload
var (fileId, url) = await _googleDriveService.UploadFileAsync(
    file, 
    fileName, 
    DriveFolderConstants.FINANCIAL_REPORTS,
    cancellationToken);

// Download
var stream = await _googleDriveService.DownloadFileAsync(fileId, cancellationToken);

// Delete
await _googleDriveService.DeleteFileAsync(fileId, cancellationToken);
```

## 🌐 Production Deployment

**Azure:**
```bash
az webapp config appsettings set --settings GOOGLE_DRIVE_CREDENTIALS="$(cat key.json)"
```

**Docker:**
```dockerfile
ENV GOOGLE_DRIVE_CREDENTIALS='{"type":"service_account",...}'
```

**Kubernetes:**
```yaml
env:
  - name: GOOGLE_DRIVE_CREDENTIALS
    valueFrom:
      secretKeyRef:
        name: google-drive-secret
        key: credentials
```

📖 **Chi tiết**: Xem [GOOGLE-DRIVE-INTEGRATION.md](GOOGLE-DRIVE-INTEGRATION.md)
