# Hướng Dẫn Tích Hợp Google Drive API

## Tổng Quan

Hệ thống đã được tích hợp Google Drive API để lưu trữ và quản lý file. Hỗ trợ nhiều loại file với kiến trúc folder linh hoạt.

## ✨ Tính Năng Chính

### 1. Authentication Linh Hoạt
- ✅ **Environment Variable (Khuyến Nghị)**: Lưu JSON credentials trong env var - an toàn cho production
- ✅ **Service Account File**: Fallback cho development local

### 2. Kiến Trúc Folder Đa Dạng
Hỗ trợ nhiều loại folder được định nghĩa trong `DriveFolderConstants`:
- 📊 **FinancialReports**: Báo cáo tài chính
- 🏢 **CompanyLogos**: Logo công ty
- 📄 **UserDocuments**: Tài liệu người dùng
- 📤 **Exports**: File export
- 📎 **Attachments**: File đính kèm
- 🖼️ **Images**: Hình ảnh
- 📋 **Templates**: Template files
- 💾 **Backups**: Backup files

### 3. Auto-Create Folders
- Tự động tạo folder nếu chưa tồn tại (configurable)
- Cache folder IDs để tối ưu performance
- Warning log khi tạo folder mới để update config

## Cấu Hình Google Drive API

### 🎯 Phương Pháp Khuyến Nghị: Environment Variable

#### Bước 1: Tạo Google Cloud Service Account (Giống cũ)

1. Truy cập [Google Cloud Console](https://console.cloud.google.com/)
2. Tạo project mới hoặc chọn project hiện có
3. Enable Google Drive API
4. Tạo Service Account và tải key JSON

#### Bước 2: Setup Environment Variable

**Development (Local):**

1. Đọc toàn bộ nội dung file JSON service account
2. Set environment variable:

**Windows PowerShell:**
```powershell
$jsonContent = Get-Content "path/to/service-account-key.json" -Raw
$env:GOOGLE_DRIVE_CREDENTIALS = $jsonContent
```

**Linux/Mac:**
```bash
export GOOGLE_DRIVE_CREDENTIALS=$(cat path/to/service-account-key.json)
```

3. Cập nhật `appsettings.Development.json`:
```json
{
  "GoogleDrive": {
    "JsonCredentials": null,  // Will be read from env var
    "RootFolderId": "YOUR_ROOT_FOLDER_ID",
    "Enabled": true,
    "AutoCreateFolders": true,
    "Folders": {
      "FinancialReports": "FOLDER_ID_1",
      "CompanyLogos": "FOLDER_ID_2"
    }
  }
}
```

**Production:**

Thêm environment variable trong hosting platform:

**Azure App Service:**
```bash
az webapp config appsettings set --name YourAppName \
  --resource-group YourResourceGroup \
  --settings GOOGLE_DRIVE_CREDENTIALS="$(cat service-account-key.json)"
```

**Docker:**
```dockerfile
ENV GOOGLE_DRIVE_CREDENTIALS='{"type":"service_account",...}'
```

**Kubernetes:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: google-drive-secret
type: Opaque
stringData:
  credentials.json: |
    {
      "type": "service_account",
      ...
    }
```

#### Bước 3: Cấu Hình Application

**Program.cs** đã tự động đọc từ env var:
```csharp
// Auto-inject from environment variable
builder.Configuration["GoogleDrive:JsonCredentials"] = 
    Environment.GetEnvironmentVariable("GOOGLE_DRIVE_CREDENTIALS");
```

Hoặc trong `appsettings.json`:
```json
{
  "GoogleDrive": {
    "JsonCredentials": null,  // From env var: GOOGLE_DRIVE_CREDENTIALS
    "ServiceAccountKeyPath": null,  // Backup option
    "RootFolderId": "YOUR_ROOT_FOLDER_ID",
    "Folders": {
      "FinancialReports": null,  // Auto-create if null
      "CompanyLogos": null
    },
    "AutoCreateFolders": true,
    "Enabled": true
  }
}
```

## 📖 API Usage Examples

### Upload File với Folder Type

```csharp
// In your handler or service
var (fileId, fileUrl) = await _googleDriveService.UploadFileAsync(
    file,
    fileName,
    DriveFolderConstants.FINANCIAL_REPORTS,  // Folder type
    cancellationToken);
```

**Available Folder Types:**
```csharp
using GreenDragonTrading.Domain.Constants;

DriveFolderConstants.FINANCIAL_REPORTS
DriveFolderConstants.COMPANY_LOGOS
DriveFolderConstants.USER_DOCUMENTS
DriveFolderConstants.EXPORTS
DriveFolderConstants.ATTACHMENTS
DriveFolderConstants.IMAGES
DriveFolderConstants.TEMPLATES
DriveFolderConstants.BACKUPS
```

## Cấu Trúc Code

### 1. DriveFolderConstants
- File: [Domain/Constants/DriveFolderConstants.cs](Domain/Constants/DriveFolderConstants.cs)
- Định nghĩa các loại folder hỗ trợ

### 2. GoogleDriveOptions
- File: [Application/Common/Options/GoogleDriveOptions.cs](Application/Common/Options/GoogleDriveOptions.cs)
- Cấu hình: JsonCredentials, RootFolderId, Folders, AutoCreateFolders

### 3. IGoogleDriveService
- File: [Application/Interfaces/IGoogleDriveService.cs](Application/Interfaces/IGoogleDriveService.cs)
- Methods: UploadFileAsync, DownloadFileAsync, DeleteFileAsync, GetOrCreateFolderAsync, CreateFolderAsync

### 4. GoogleDriveService
- File: [Infrastructure/Services/GoogleDriveService.cs](Infrastructure/Services/GoogleDriveService.cs)
- Hỗ trợ JSON credentials từ env var hoặc file path
- Auto-create folders với caching
- Folder type-based upload

### 5. UploadFileCommandHandler
- File: [Application/UseCases/FinancialReports/Commands/UploadFile/UploadFileCommandHandler.cs](Application/UseCases/FinancialReports/Commands/UploadFile/UploadFileCommandHandler.cs)
- Sử dụng `DriveFolderConstants.FINANCIAL_REPORTS`

### 6. DownloadFileQuery/Handler
- Files: 
  - [Application/UseCases/FinancialReports/Queries/DownloadFile/DownloadFileQuery.cs](Application/UseCases/FinancialReports/Queries/DownloadFile/DownloadFileQuery.cs)
  - [Application/UseCases/FinancialReports/Queries/DownloadFile/DownloadFileQueryHandler.cs](Application/UseCases/FinancialReports/Queries/DownloadFile/DownloadFileQueryHandler.cs)

## 🔐 Bảo Mật

### ✅ Best Practices (Sử Dụng Env Var)

1. **Không commit credentials vào Git**
2. **Sử dụng Environment Variables cho production**
3. **Rotate service account keys định kỳ**
4. **Chỉ share folder cần thiết với service account**
5. **Set quyền "Editor" (không dùng "Owner")**

### ⚠️ Legacy Method (File-based)

Nếu vẫn muốn dùng file (không khuyến nghị):
```json
{
  "GoogleDrive": {
    "ServiceAccountKeyPath": "ServiceAccounts/service-account.json",
    "JsonCredentials": null
  }
}
```

**Thêm vào .gitignore:**
```gitignore
# Service Account Keys
**/ServiceAccounts/**/*.json
```

## ❗ Troubleshooting

### "Google Drive credentials not configured"
✅ **Solution**: Set environment variable `GOOGLE_DRIVE_CREDENTIALS` với nội dung JSON file

### "Google Drive integration is disabled"
✅ **Solution**: Set `GoogleDrive:Enabled: true` trong appsettings

### "Folder type 'XXX' is not configured and AutoCreateFolders is disabled"
✅ **Solution**: 
- Bật `AutoCreateFolders: true`, hoặc
- Thêm folder ID vào config: `GoogleDrive:Folders:XXX`

### "The caller does not have permission"
✅ **Solution**: 
- Share root folder với service account email
- Đảm bảo role là "Editor"

### File upload thành công nhưng không thấy trong folder
✅ **Solution**: 
- Kiểm tra folder ID đúng chưa
- Xem log để lấy folder ID thực tế được tạo

## 🚀 Quick Start

### Development Setup (3 phút)

1. **Download service account JSON từ Google Cloud**

2. **Set environment variable:**
   ```powershell
   $env:GOOGLE_DRIVE_CREDENTIALS = Get-Content "service-account-key.json" -Raw
   ```

3. **Tạo root folder trên Drive, share với service account**

4. **Update appsettings.Development.json:**
   ```json
   {
     "GoogleDrive": {
       "RootFolderId": "YOUR_ROOT_FOLDER_ID",
       "AutoCreateFolders": true,
       "Enabled": true
     }
   }
   ```

5. **Run app** - Folders sẽ tự động tạo!

### Production Deployment

1. **Set env var trên hosting platform:**
   ```bash
   GOOGLE_DRIVE_CREDENTIALS='{"type":"service_account",...}'
   ```

2. **Configure appsettings.Production.json:**
   ```json
   {
     "GoogleDrive": {
       "RootFolderId": "PRODUCTION_ROOT_FOLDER_ID",
       "AutoCreateFolders": false,
       "Folders": {
         "FinancialReports": "FOLDER_ID_1",
         "CompanyLogos": "FOLDER_ID_2"
       }
     }
   }
   ```

## 🎯 Thêm Folder Type Mới

1. **Add constant:**
   ```csharp
   // In DriveFolderConstants.cs
   public const string MY_NEW_FOLDER = "MyNewFolder";
   ```

2. **Use in code:**
   ```csharp
   await _googleDriveService.UploadFileAsync(
       file, 
       fileName, 
       DriveFolderConstants.MY_NEW_FOLDER,
       cancellationToken);
   ```

3. **Auto-create hoặc config folder ID** (tùy chọn)

## 📊 Migration Từ Hệ Thống Cũ

Nếu có file trong S3/local storage:

```csharp
// Pseudo code
foreach (var oldFile in oldFiles)
{
    // Download from old storage
    var stream = await oldStorage.DownloadAsync(oldFile.Path);
    
    // Upload to Google Drive
    var (fileId, url) = await _googleDriveService.UploadFileFromStreamAsync(
        stream, 
        oldFile.Name, 
        oldFile.MimeType,
        DriveFolderConstants.FINANCIAL_REPORTS,
        cancellationToken);
    
    // Update database
    entity.FilePath = fileId;
    await _uow.SaveChangesAsync();
    
    // Optional: Delete old file
    await oldStorage.DeleteAsync(oldFile.Path);
}
```

## 📦 Package Dependencies

- `Google.Apis.Drive.v3` - Version 1.73.0.3996

## 🧪 Testing Checklist

- [ ] Environment variable được set đúng
- [ ] Upload file mới vào folder type
- [ ] Kiểm tra file hiển thị đúng folder trên Drive
- [ ] Download file và verify nội dung
- [ ] Upload file mới để replace (file cũ bị xóa)
- [ ] Test với nhiều folder types khác nhau
- [ ] Test auto-create folders
- [ ] Test với file size lớn
- [ ] Test với các định dạng file khác nhau

## ⚡ Performance & Best Practices

1. **Folder caching**: Service tự động cache folder IDs
2. **Shareable links**: Dùng direct link thay vì download qua API khi có thể
3. **Rate limiting**: Google Drive API có giới hạn, implement retry nếu cần
4. **Parallel uploads**: Có thể upload nhiều files song song
5. **Cleanup**: Xóa file cũ khi không cần nữa

## 🔮 Future Enhancements

- [ ] Chunked upload cho file > 100MB
- [ ] Batch operations
- [ ] Folder hierarchy động (theo ticker/year)
- [ ] Webhook từ Google Drive
- [ ] File versioning
- [ ] Shared access per user
- [ ] Activity logging
