# Financial Report API Documentation

## Tổng quan

API Financial Report đã được cải tiến để cho phép người dùng nhập tay từng trường dữ liệu tài chính thay vì chỉ upload file. Hệ thống lưu trữ dữ liệu có cấu trúc cụ thể trong database PostgreSQL.

## Cấu trúc dữ liệu

### Balance Sheet (Bảng cân đối kế toán)
- `totalAssets`: Tổng tài sản
- `totalLiabilities`: Tổng nợ phải trả
- `ownerEquity`: Vốn chủ sở hữu
- `currentAssets`: Tài sản ngắn hạn
- `currentLiabilities`: Nợ ngắn hạn
- `shortTermInvestments`: Đầu tư ngắn hạn
- `longTermAssets`: Tài sản dài hạn
- `inventory`: Hàng tồn kho

### Income Statement (Báo cáo kết quả kinh doanh)
- `revenue`: Doanh thu
- `grossProfit`: Lợi nhuận gộp
- `operatingProfit`: Lợi nhuận hoạt động
- `profitBeforeTax`: Lợi nhuận trước thuế
- `profitAfterTax`: Lợi nhuận sau thuế
- `netIncome`: Thu nhập ròng
- `costOfGoodsSold`: Giá vốn hàng bán
- `operatingExpenses`: Chi phí hoạt động

### Cash Flow Statement (Báo cáo lưu chuyển tiền tệ)
- `cashFromOperating`: Tiền từ hoạt động kinh doanh
- `cashFromInvesting`: Tiền từ hoạt động đầu tư
- `cashFromFinancing`: Tiền từ hoạt động tài chính
- `netCashFlow`: Lưu chuyển tiền thuần
- `beginningCash`: Tiền đầu kỳ
- `endingCash`: Tiền cuối kỳ

### Financial Ratios (Các chỉ số tài chính)
- `eps`: Thu nhập trên mỗi cổ phiếu (Earnings Per Share)
- `roe`: Tỷ suất sinh lời trên vốn chủ sở hữu (Return on Equity)
- `roa`: Tỷ suất sinh lời trên tổng tài sản (Return on Assets)
- `debtToEquity`: Tỷ lệ nợ trên vốn chủ sở hữu
- `currentRatio`: Tỷ số thanh toán hiện hành
- `quickRatio`: Tỷ số thanh toán nhanh
- `grossMargin`: Biên lợi nhuận gộp
- `operatingMargin`: Biên lợi nhuận hoạt động
- `netMargin`: Biên lợi nhuận ròng

### Metadata
- `notes`: Ghi chú bổ sung
- `file`: File đính kèm (PDF, Excel, hoặc hình ảnh - tùy chọn)

## API Endpoints

### 1. Tạo báo cáo tài chính mới (Create)

**Endpoint:** `POST /api/v1/financial-reports`

**Authorization:** Required (Bearer Token)

**Content-Type:** `multipart/form-data`

**Request Body:**
```json
{
  "ticker": "VNM",
  "year": 2024,
  "period": 4,
  "file": "<optional file upload>",
  
  // Balance Sheet
  "totalAssets": 50000000000,
  "totalLiabilities": 20000000000,
  "ownerEquity": 30000000000,
  "currentAssets": 25000000000,
  "currentLiabilities": 10000000000,
  "shortTermInvestments": 5000000000,
  "longTermAssets": 25000000000,
  "inventory": 8000000000,
  
  // Income Statement
  "revenue": 40000000000,
  "grossProfit": 15000000000,
  "operatingProfit": 10000000000,
  "profitBeforeTax": 9000000000,
  "profitAfterTax": 7200000000,
  "netIncome": 7200000000,
  "costOfGoodsSold": 25000000000,
  "operatingExpenses": 5000000000,
  
  // Cash Flow Statement
  "cashFromOperating": 8000000000,
  "cashFromInvesting": -2000000000,
  "cashFromFinancing": -1000000000,
  "netCashFlow": 5000000000,
  "beginningCash": 3000000000,
  "endingCash": 8000000000,
  
  // Financial Ratios
  "eps": 4500,
  "roe": 0.24,
  "roa": 0.144,
  "debtToEquity": 0.67,
  "currentRatio": 2.5,
  "quickRatio": 1.7,
  "grossMargin": 0.375,
  "operatingMargin": 0.25,
  "netMargin": 0.18,
  
  // Metadata
  "notes": "Báo cáo tài chính Q4/2024"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Tạo báo cáo tài chính thành công",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "ticker": "VNM",
    "year": 2024,
    "period": 4,
    "filePath": "/financial-reports/2024/VNM/report.pdf",
    "fileUrl": "https://r2.example.com/financial-reports/2024/VNM/report.pdf",
    "fileSize": 1024000,
    "contentType": "application/pdf",
    "totalAssets": 50000000000,
    // ... all other fields
    "status": 2,
    "createdAt": "2024-01-11T05:30:00Z",
    "updatedAt": "2024-01-11T05:30:00Z"
  },
  "responseTime": "2024-01-11T05:30:00Z"
}
```

### 2. Cập nhật báo cáo tài chính (Update)

**Endpoint:** `PUT /api/v1/financial-reports/{id}`

**Authorization:** Required (Bearer Token)

**Content-Type:** `multipart/form-data`

**Request Body:** (Giống như Create, nhưng tất cả các trường đều optional)

**Response:** Tương tự Create

### 3. Lấy danh sách báo cáo tài chính (List with filters)

**Endpoint:** `GET /api/v1/financial-reports`

**Authorization:** Not required

**Query Parameters:**
- `ticker` (string, optional): Lọc theo mã chứng khoán
- `year` (int, optional): Lọc theo năm
- `period` (int, optional): Lọc theo kỳ (1-4: Q1-Q4, 5: Yearly)
- `status` (int, optional): Lọc theo trạng thái (0: Pending, 1: Processing, 2: Completed, 3: Failed, 4: Archived)
- `pageIndex` (int, required): Trang hiện tại (bắt đầu từ 1)
- `pageSize` (int, required): Số lượng bản ghi mỗi trang (max 100)

**Example:**
```
GET /api/v1/financial-reports?ticker=VNM&year=2024&pageIndex=1&pageSize=10
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Lấy danh sách báo cáo tài chính thành công",
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "ticker": "VNM",
        "year": 2024,
        "period": 4,
        // ... all fields
      }
    ],
    "pageIndex": 1,
    "totalPages": 5,
    "totalCount": 45,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "responseTime": "2024-01-11T05:30:00Z"
}
```

### 4. Lấy chi tiết một báo cáo tài chính (Get by ID)

**Endpoint:** `GET /api/v1/financial-reports/{id}`

**Authorization:** Not required

**Response:** Giống như Create response

### 5. Xóa báo cáo tài chính (Delete)

**Endpoint:** `DELETE /api/v1/financial-reports/{id}`

**Authorization:** Required (Bearer Token)

**Response:**
```json
{
  "isSuccess": true,
  "message": "Xóa báo cáo tài chính thành công",
  "responseTime": "2024-01-11T05:30:00Z"
}
```

### 6. Upload file (Legacy endpoint - chỉ upload file)

**Endpoint:** `POST /api/v1/financial-reports/upload`

**Authorization:** Required (Bearer Token)

**Content-Type:** `multipart/form-data`

**Request Body:**
```json
{
  "ticker": "VNM",
  "year": 2024,
  "period": 4,
  "file": "<file upload>"
}
```

**Note:** Endpoint này chỉ để upload file, không nhập dữ liệu chi tiết. Sử dụng endpoint Create để tạo báo cáo đầy đủ.

## Validation Rules

### File Upload
- Kích thước tối đa: 50MB
- Định dạng được phép: PDF, Excel (xlsx/xls), hình ảnh (jpg/png)

### Required Fields
- `ticker`: Bắt buộc, tối đa 20 ký tự
- `year`: Bắt buộc, > 1990 và <= năm hiện tại + 1
- `period`: Bắt buộc, phải thuộc enum ReportPeriod (1-5)
- Ít nhất một trường dữ liệu tài chính phải được điền

### Data Types
- Tất cả các trường số tài chính: `decimal` (nullable)
- Tất cả các tỷ lệ/tỷ số: `decimal` (nullable)
- Notes: `string` (nullable)

## Enums

### ReportPeriod
```csharp
Q1 = 1      // Quý 1
Q2 = 2      // Quý 2
Q3 = 3      // Quý 3
Q4 = 4      // Quý 4
Yearly = 5  // Cả năm
```

### FinancialReportStatus
```csharp
Pending = 0       // Chờ xử lý
Processing = 1    // Đang xử lý
Completed = 2     // Hoàn thành
Failed = 3        // Thất bại
Archived = 4      // Đã lưu trữ
```

## Migration

Migration đã được tạo: `UpdateFinancialReportWithDetailedFields`

**Áp dụng migration:**
```bash
dotnet ef database update --project GreenDragonTrading.Infrastructure --startup-project GreenDragonTrading.Api
```

## Notes

1. Tất cả các trường số tài chính đều là nullable, người dùng có thể nhập từng phần
2. File đính kèm là tùy chọn, có thể tạo báo cáo chỉ với dữ liệu nhập tay
3. Status mặc định khi tạo mới là `Completed` (vì dữ liệu đã được nhập tay)
4. Khi xóa báo cáo, file đính kèm (nếu có) cũng sẽ được xóa khỏi R2 storage
5. Khi cập nhật và có file mới, file cũ sẽ bị thay thế
