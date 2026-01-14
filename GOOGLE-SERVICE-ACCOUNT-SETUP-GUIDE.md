# 📚 Hướng Dẫn Chi Tiết Setup Google Service Account

## Mục Lục
1. [Tạo Google Cloud Project](#1-tạo-google-cloud-project)
2. [Enable Google Drive API](#2-enable-google-drive-api)
3. [Tạo Service Account](#3-tạo-service-account)
4. [Tạo và Download Key](#4-tạo-và-download-key)
5. [Setup Folder trên Google Drive](#5-setup-folder-trên-google-drive)
6. [Cấu hình trong Application](#6-cấu-hình-trong-application)
7. [Testing](#7-testing)
8. [Troubleshooting](#8-troubleshooting)

---

## 1. Tạo Google Cloud Project

### Bước 1.1: Truy cập Google Cloud Console

1. Mở browser và truy cập: https://console.cloud.google.com/
2. Đăng nhập bằng Google Account (sử dụng account organization/work nếu có)

### Bước 1.2: Tạo Project Mới

1. Click vào **dropdown project** ở góc trên bên trái (bên cạnh "Google Cloud")
2. Trong popup hiện ra, click **"NEW PROJECT"** (góc trên bên phải)

3. Điền thông tin project:
   ```
   Project name: GreenDragonTrading
   Organization: (Chọn organization nếu có, hoặc để "No organization")
   Location: (Chọn organization folder nếu có, hoặc để trống)
   ```

4. Click **"CREATE"**

5. Đợi 10-30 giây để Google tạo project

6. Sau khi tạo xong, click vào **notification bell** (góc trên phải) để xem notification
   - Hoặc chọn project từ dropdown

### Bước 1.3: Verify Project đã được tạo

1. Kiểm tra dropdown project hiện tên: **"GreenDragonTrading"**
2. URL sẽ có dạng: `https://console.cloud.google.com/home/dashboard?project=greendragontrading-xxxxx`
3. Note lại **Project ID** (dạng: `greendragontrading-123456`) - sẽ cần sau này

---

## 2. Enable Google Drive API

### Bước 2.1: Vào APIs & Services

1. Từ Console, click **☰ menu** (góc trên trái)
2. Scroll xuống tìm **"APIs & Services"**
3. Click vào **"Library"** (Thư viện API)

### Bước 2.2: Tìm và Enable Google Drive API

1. Trong search box, gõ: **"Google Drive API"**
2. Click vào kết quả đầu tiên: **"Google Drive API"** (by Google)
3. Trên trang API detail, click nút **"ENABLE"** (màu xanh)
4. Đợi 5-10 giây
5. Sau khi enable, bạn sẽ thấy màn hình API metrics

### Bước 2.3: Verify API đã được enable

1. Click **☰ menu** → **"APIs & Services"** → **"Enabled APIs & services"**
2. Trong danh sách, tìm **"Google Drive API"** với status **"Enabled"**
3. Click vào để xem quotas và usage (optional)

---

## 3. Tạo Service Account

### Bước 3.1: Navigate to Service Accounts

1. Click **☰ menu** → **"APIs & Services"** → **"Credentials"**
2. Trên trang Credentials, click tab **"Service Accounts"** (nếu có)
   - Hoặc tìm section "Service Accounts" phía dưới

### Bước 3.2: Create Service Account

1. Click nút **"+ CREATE SERVICE ACCOUNT"** (ở trên)
2. Điền thông tin **Service account details**:

   ```
   Service account name: greendragontrading-drive
   Service account ID: greendragontrading-drive (auto-generated từ name)
   Service account description: Service account for managing files on Google Drive
   ```

3. Click **"CREATE AND CONTINUE"**

### Bước 3.3: Grant Permissions (Optional - có thể skip)

1. Ở bước **"Grant this service account access to project"**:
   - **Select a role**: Để trống hoặc chọn **"Basic"** → **"Viewer"** (không cần role cao)
   - Click **"CONTINUE"**

2. Ở bước **"Grant users access to this service account"** (optional):
   - Để trống
   - Click **"DONE"**

### Bước 3.4: Verify Service Account đã tạo

1. Bạn sẽ thấy service account trong danh sách:
   ```
   Name: greendragontrading-drive
   Email: greendragontrading-drive@greendragontrading-xxxxx.iam.gserviceaccount.com
   Status: Enabled
   ```

2. **QUAN TRỌNG**: Copy **email** này (click vào email để copy)
   - Ví dụ: `greendragontrading-drive@greendragontrading-123456.iam.gserviceaccount.com`
   - Save vào notepad, sẽ cần ở bước 5

---

## 4. Tạo và Download Key

### Bước 4.1: Access Service Account Keys

1. Trong danh sách Service Accounts, click vào **email** của service account vừa tạo
2. Bạn sẽ vào trang **Service account details**

### Bước 4.2: Create New Key

1. Click tab **"KEYS"** (ở trên, bên cạnh DETAILS, PERMISSIONS, METRICS)
2. Click **"ADD KEY"** dropdown → chọn **"Create new key"**

3. Popup hiện ra chọn key type:
   ```
   ○ P12
   ● JSON (chọn cái này)
   ```

4. Click **"CREATE"**

### Bước 4.3: Download Key File

1. File JSON sẽ tự động download về máy bạn
   - Tên file dạng: `greendragontrading-xxxxx-xxxxxxxxx.json`
   - Hoặc: `greendragontrading-drive-credentials.json`

2. **QUAN TRỌNG - BẢO MẬT**:
   - ⚠️ File này chứa private key - KHÔNG CHIA SẺ VỚI AI
   - ⚠️ KHÔNG commit vào Git
   - ⚠️ Lưu ở nơi an toàn
   - ✅ Rename thành `google-drive-service-account.json` cho dễ nhớ
   - ✅ Move vào thư mục riêng, ví dụ: `D:\Credentials\` hoặc `~/credentials/`

### Bước 4.4: Verify Key File

1. Mở file JSON bằng text editor (Notepad++, VS Code, etc.)
2. Kiểm tra có các fields quan trọng:
   ```json
   {
     "type": "service_account",
     "project_id": "greendragontrading-xxxxx",
     "private_key_id": "abc123...",
     "private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvQ...\n-----END PRIVATE KEY-----\n",
     "client_email": "greendragontrading-drive@greendragontrading-xxxxx.iam.gserviceaccount.com",
     "client_id": "123456789...",
     "auth_uri": "https://accounts.google.com/o/oauth2/auth",
     "token_uri": "https://oauth2.googleapis.com/token",
     ...
   }
   ```

3. Verify `client_email` match với email đã copy ở bước 3.4

### Bước 4.5: Security Warning từ Google

1. Sau khi tạo key, Google sẽ hiển thị warning:
   ```
   ⚠️ Your private key has been created and downloaded.
   Keep it secure, as it can be used to authenticate as your service account.
   ```

2. Google khuyến nghị:
   - Rotate keys định kỳ (mỗi 90 ngày)
   - Không share keys
   - Monitor key usage

---

## 5. Setup Folder trên Google Drive

### Bước 5.1: Login vào Google Drive

1. Truy cập: https://drive.google.com/
2. Login bằng **Google Account** của bạn (có thể khác account tạo project)
   - Nên dùng account organization/work để quản lý chung

### Bước 5.2: Tạo Root Folder

1. Click **"+ New"** (góc trái) → **"Folder"**
2. Đặt tên folder: **"GreenDragonTrading"**
3. Click **"Create"**

### Bước 5.3: Tạo Sub-folders (Recommended)

1. Double-click vào folder **"GreenDragonTrading"** vừa tạo
2. Tạo các sub-folders:

   Click **"+ New"** → **"Folder"** cho mỗi folder sau:
   ```
   ├── FinancialReports
   ├── CompanyLogos
   ├── UserDocuments
   ├── Exports
   ├── Attachments
   ├── Images
   ├── Templates
   └── Backups
   ```

3. Sau khi tạo xong, bạn sẽ có structure:
   ```
   📁 GreenDragonTrading/
      📁 FinancialReports/
      📁 CompanyLogos/
      📁 UserDocuments/
      📁 Exports/
      📁 Attachments/
      📁 Images/
      📁 Templates/
      📁 Backups/
   ```

### Bước 5.4: Share Root Folder với Service Account

1. **QUAN TRỌNG**: Right-click vào folder **"GreenDragonTrading"** (root folder)
2. Click **"Share"**

3. Trong popup **"Share with people and groups"**:
   - Paste **service account email** đã copy ở bước 3.4
   - Email dạng: `greendragontrading-drive@greendragontrading-xxxxx.iam.gserviceaccount.com`

4. Chọn role:
   ```
   Viewer  ❌ (chỉ xem)
   Commenter  ❌ (chỉ comment)
   Editor  ✅ (CHỌN CÁI NÀY - upload/delete files)
   ```

5. **Bỏ tick** "Notify people" (service account không cần email notification)

6. Click **"Share"** hoặc **"Send"**

### Bước 5.5: Copy Folder IDs

Bạn cần copy ID của các folders để config trong app.

**Cách 1: Từ URL**

1. Click vào folder **"GreenDragonTrading"**
2. Check URL trên browser:
   ```
   https://drive.google.com/drive/folders/1a2b3c4d5e6f7g8h9i0j1k2l3m4n5o6p
                                            └────────────────────────────┘
                                                    Folder ID
   ```
3. Copy phần sau `/folders/` → đó là **Root Folder ID**
   - Ví dụ: `1a2b3c4d5e6f7g8h9i0j1k2l3m4n5o6p`

4. Lặp lại với từng sub-folder:
   - Click vào **FinancialReports** → copy ID
   - Click vào **CompanyLogos** → copy ID
   - ... (tương tự)

**Cách 2: Right-click → Get Link**

1. Right-click vào folder → **"Get link"**
2. Link dạng: `https://drive.google.com/drive/folders/FOLDER_ID?usp=sharing`
3. Copy phần `FOLDER_ID`

### Bước 5.6: Save Folder IDs

Lưu vào notepad theo format:

```
Root: 1a2b3c4d5e6f7g8h9i0j1k2l3m4n5o6p
FinancialReports: 1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q
CompanyLogos: 1c2d3e4f5g6h7i8j9k0l1m2n3o4p5q6r
UserDocuments: 1d2e3f4g5h6i7j8k9l0m1n2o3p4q5r6s
Exports: 1e2f3g4h5i6j7k8l9m0n1o2p3q4r5s6t
Attachments: 1f2g3h4i5j6k7l8m9n0o1p2q3r4s5t6u
Images: 1g2h3i4j5k6l7m8n9o0p1q2r3s4t5u6v
Templates: 1h2i3j4k5l6m7n8o9p0q1r2s3t4u5v6w
Backups: 1i2j3k4l5m6n7o8p9q0r1s2t3u4v5w6x
```

---

## 6. Cấu hình trong Application

### Bước 6.1: Setup Environment Variable (Development)

**Windows (PowerShell):**

```powershell
# Navigate đến folder chứa JSON key
cd D:\Credentials\

# Read file và set env var
$jsonContent = Get-Content "google-drive-service-account.json" -Raw
$env:GOOGLE_DRIVE_CREDENTIALS = $jsonContent

# Verify
echo $env:GOOGLE_DRIVE_CREDENTIALS
```

**Linux/Mac (Bash):**

```bash
# Navigate
cd ~/credentials/

# Set env var
export GOOGLE_DRIVE_CREDENTIALS=$(cat google-drive-service-account.json)

# Verify
echo $GOOGLE_DRIVE_CREDENTIALS | jq .
```

**Permanent (Add to profile):**

Windows - Thêm vào PowerShell profile:
```powershell
# Edit profile
notepad $PROFILE

# Add line:
$env:GOOGLE_DRIVE_CREDENTIALS = Get-Content "D:\Credentials\google-drive-service-account.json" -Raw
```

Linux/Mac - Thêm vào ~/.bashrc or ~/.zshrc:
```bash
export GOOGLE_DRIVE_CREDENTIALS=$(cat ~/credentials/google-drive-service-account.json)
```

### Bước 6.2: Cấu hình appsettings.Development.json

1. Mở file: `GreenDragonTrading.Api/appsettings.Development.json`

2. Thêm/Update section GoogleDrive:

```json
{
  "GoogleDrive": {
    "JsonCredentials": null,
    "ServiceAccountKeyPath": null,
    "ApplicationName": "GreenDragonTrading",
    "RootFolderId": "1a2b3c4d5e6f7g8h9i0j1k2l3m4n5o6p",
    "Folders": {
      "FinancialReports": "1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q",
      "CompanyLogos": "1c2d3e4f5g6h7i8j9k0l1m2n3o4p5q6r",
      "UserDocuments": "1d2e3f4g5h6i7j8k9l0m1n2o3p4q5r6s",
      "Exports": "1e2f3g4h5i6j7k8l9m0n1o2p3q4r5s6t",
      "Attachments": "1f2g3h4i5j6k7l8m9n0o1p2q3r4s5t6u",
      "Images": "1g2h3i4j5k6l7m8n9o0p1q2r3s4t5u6v",
      "Templates": "1h2i3j4k5l6m7n8o9p0q1r2s3t4u5v6w",
      "Backups": "1i2j3k4l5m6n7o8p9q0r1s2t3u4v5w6x"
    },
    "Enabled": true,
    "MaxFileSizeBytes": 52428800,
    "AutoCreateFolders": false
  }
}
```

3. **Thay thế** các Folder IDs bằng IDs thực tế từ bước 5.6

4. Note:
   - `JsonCredentials: null` → Sẽ đọc từ env var
   - `AutoCreateFolders: false` → Vì đã tạo manual

### Bước 6.3: Verify Configuration

```csharp
// Check trong Program.cs logs
// Khi start app, sẽ thấy log:
// "Google Drive credentials loaded from environment variable"
```

---

## 7. Testing

### Bước 7.1: Start Application

```powershell
# From solution root
cd D:\FPT\CAPSTONE\GreenDragonTrading\src\GreenDragonTrading.dotnet

# Set env var (if not permanent)
$env:GOOGLE_DRIVE_CREDENTIALS = Get-Content "D:\Credentials\google-drive-service-account.json" -Raw

# Run
dotnet run --project GreenDragonTrading.Api
```

### Bước 7.2: Check Logs

Tìm trong console logs:

```
✅ "Google Drive credentials loaded from environment variable"
✅ "Initializing Google Drive with JSON credentials from environment variable"
```

Nếu có lỗi:
```
❌ "Google Drive credentials not configured"
→ Env var chưa set hoặc JSON không hợp lệ
```

### Bước 7.3: Test Upload File

**Sử dụng Swagger UI:**

1. Mở browser: `https://localhost:7148/swagger`
2. Tìm endpoint: `POST /api/v1/financial-reports/{id}/upload-file`
3. Click **"Try it out"**
4. Nhập:
   - `id`: Guid của financial report có sẵn
   - `file`: Chọn file PDF/Excel để upload
5. Click **"Execute"**

**Expected Response (Success):**
```json
{
  "isSuccess": true,
  "message": "Upload file thành công.",
  "data": {
    "id": "...",
    "filePath": "1x2y3z4...",  // Google Drive File ID
    "fileUrl": "https://drive.google.com/file/d/1x2y3z4.../view?usp=sharing",
    ...
  }
}
```

### Bước 7.4: Verify trên Google Drive

1. Quay lại Google Drive: https://drive.google.com/
2. Navigate: **GreenDragonTrading** → **FinancialReports**
3. Bạn sẽ thấy file vừa upload với tên dạng: `VNM_2023_Q4.pdf`

### Bước 7.5: Test Download File

**Swagger UI:**

1. Endpoint: `GET /api/v1/financial-reports/{id}/download-file`
2. Nhập `id` của report vừa upload
3. Click **"Execute"**
4. File sẽ download về máy

**Hoặc dùng browser trực tiếp:**

Copy `fileUrl` từ response → paste vào browser → File sẽ hiển thị/download

---

## 8. Troubleshooting

### ❌ Error: "Google Drive credentials not configured"

**Nguyên nhân:**
- Environment variable chưa được set
- JSON content không hợp lệ

**Fix:**

```powershell
# Check env var
echo $env:GOOGLE_DRIVE_CREDENTIALS

# Nếu null → set lại
$env:GOOGLE_DRIVE_CREDENTIALS = Get-Content "path\to\key.json" -Raw

# Restart app
```

---

### ❌ Error: "The caller does not have permission"

**Nguyên nhân:**
- Folder chưa được share với service account
- Role không đúng (Viewer thay vì Editor)

**Fix:**

1. Vào Google Drive
2. Right-click folder → Share
3. Thêm service account email với role **Editor**
4. Retry upload

---

### ❌ Error: "Folder type 'FinancialReports' is not configured"

**Nguyên nhân:**
- Folder ID trong appsettings.json không đúng hoặc null

**Fix:**

```json
// appsettings.Development.json
{
  "GoogleDrive": {
    "Folders": {
      "FinancialReports": "PASTE_CORRECT_FOLDER_ID_HERE"
    }
  }
}
```

---

### ❌ Error: "Invalid JSON format"

**Nguyên nhân:**
- JSON credentials bị escape sai khi set env var
- Có ký tự đặc biệt không được escape

**Fix:**

```powershell
# Dùng single quotes
$env:GOOGLE_DRIVE_CREDENTIALS = Get-Content "key.json" -Raw

# Verify JSON valid
$env:GOOGLE_DRIVE_CREDENTIALS | ConvertFrom-Json
```

---

### ❌ File upload thành công nhưng không thấy trong folder

**Nguyên nhân:**
- Folder ID sai
- Service account upload vào folder khác

**Debug:**

1. Check logs để lấy file ID uploaded:
   ```
   Successfully uploaded file VNM_2023_Q4.pdf with ID 1x2y3z4...
   ```

2. Vào Google Drive search file ID:
   - Paste file ID vào search box
   - Xem file nằm ở folder nào

3. Update lại Folder ID đúng trong config

---

### ⚠️ Warning: Key rotation reminder

Google khuyến nghị rotate service account keys mỗi 90 ngày.

**Cách rotate:**

1. Tạo key mới (Bước 4)
2. Update env var với JSON key mới
3. Test upload works
4. Delete key cũ trong Google Cloud Console:
   - Service Account → Keys → Click key cũ → Delete

---

## 📋 Checklist Hoàn Tất

Copy checklist này vào task manager:

- [ ] ✅ Tạo Google Cloud Project
- [ ] ✅ Enable Google Drive API
- [ ] ✅ Tạo Service Account
- [ ] ✅ Download JSON key file
- [ ] ✅ Lưu JSON file ở nơi an toàn
- [ ] ✅ Tạo root folder trên Google Drive
- [ ] ✅ Tạo sub-folders
- [ ] ✅ Share root folder với service account email (role: Editor)
- [ ] ✅ Copy tất cả folder IDs
- [ ] ✅ Set environment variable `GOOGLE_DRIVE_CREDENTIALS`
- [ ] ✅ Update appsettings.Development.json với folder IDs
- [ ] ✅ Start application và check logs
- [ ] ✅ Test upload file
- [ ] ✅ Verify file hiện trên Google Drive
- [ ] ✅ Test download file
- [ ] ✅ Add reminder rotate keys sau 90 ngày

---

## 🎯 Quick Reference

**Service Account Email:**
```
greendragontrading-drive@greendragontrading-xxxxx.iam.gserviceaccount.com
```

**Folder Structure:**
```
GreenDragonTrading/
├── FinancialReports/
├── CompanyLogos/
├── UserDocuments/
├── Exports/
├── Attachments/
├── Images/
├── Templates/
└── Backups/
```

**Set Env Var (PowerShell):**
```powershell
$env:GOOGLE_DRIVE_CREDENTIALS = Get-Content "path\to\key.json" -Raw
```

**Set Env Var (Bash):**
```bash
export GOOGLE_DRIVE_CREDENTIALS=$(cat path/to/key.json)
```

**Start App:**
```bash
dotnet run --project GreenDragonTrading.Api
```

**Test Endpoint:**
```
POST https://localhost:7148/api/v1/financial-reports/{id}/upload-file
```

---

## 📞 Support Links

- Google Cloud Console: https://console.cloud.google.com/
- Google Drive: https://drive.google.com/
- API Library: https://console.cloud.google.com/apis/library
- Service Accounts: https://console.cloud.google.com/iam-admin/serviceaccounts
- Drive API Quotas: https://console.cloud.google.com/apis/api/drive.googleapis.com/quotas

---

**🎉 Chúc mừng! Bạn đã setup thành công Google Drive với Service Account!**
