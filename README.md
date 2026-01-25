# POSv01 — C# WinForms 收銀系統（.NET + EF Core + SQLite）

一套以 **C# WinForms** 開發的 POS 收銀系統，採用 **SQLite + Entity Framework Core** 作為資料存取，並以 **Domain / Services / Infrastructure** 分層架構設計。  
除了 UI 操作外，也完整實作了後端常見的「交易一致性（Transaction/Rollback）」、「Use Case Service 流程封裝」、「查詢效能優化（AsNoTracking + DTO）」等設計，具備可移植至 **ASP.NET Core Web API** 的後端工程思維。

---

## 📸 系統畫面 / Demo


### 主收銀台畫面

<img width="1171" height="607" alt="圖片2" src="https://github.com/user-attachments/assets/6d8c3d1e-aef1-4940-bdeb-186317260980" />


### 商品選擇（快速加入購物車）
<img width="1155" height="632" alt="圖片1" src="https://github.com/user-attachments/assets/9ee84a08-06aa-4369-be6b-c348c52d3006" />


### 訂單查詢 / 退貨查詢
<img width="925" height="437" alt="圖片5" src="https://github.com/user-attachments/assets/30a66a3b-f770-4a0d-a21f-f344e3ef03fc" />

### 收據預覽
<img width="539" height="618" alt="圖片6" src="https://github.com/user-attachments/assets/2ed14983-e35c-4c98-8c0b-41abd96fae0a" />

---

## ⭐ 專案亮點

- ✅ **交易一致性（Transaction + Rollback）**  
  結帳 / 退貨流程使用交易機制保證 **Sale + SaleItems + 庫存更新** 要嘛全部成功，要嘛全部回滾，避免資料只寫一半造成帳務錯誤。

- ✅ **Use Case Service 流程封裝（後端導向設計）**  
  UI 是最容易變動的部分，但 Service 層是核心。我把交易流程封裝在 Service，並用 Transaction 確保一致性，所以未來換成 Web / Mobile 都可以沿用核心邏輯。

- ✅ **支援部分退貨 + 商業規則驗證**  
  退貨流程支援「部分退貨」，並驗證「退貨數量不得超過原購買數量」，確保交易正確性。

- ✅ **查詢效能優化（報表 / 歷史紀錄）**  
  查詢服務使用 `AsNoTracking()`、條件篩選（日期 / 單號 / 關鍵字）、DTO 投影、分頁/限制筆數，確保查詢快速且安全。

- ✅ **資料完整性與效能（Unique + Index）**  
  `SaleNo` 設為唯一（Unique），並對 `CreatedAt` 建立索引（Index），加速查詢與報表使用情境。

- ✅ **WinForms 實戰細節（穩定性與除錯能力）**  
  具備「設計模式安全初始化（避免 Designer 爆掉）」與「事件只綁一次（避免 Click 觸發兩次）」等實務處理。

---

## 🧰 技術架構與工具

- **語言**：C#  
- **前端 UI**：WinForms  
- **資料庫**：SQLite  
- **ORM**：Entity Framework Core（Migration）  
- **架構**：Domain / Services / Infrastructure 分層 + Query Services + DTO  
- **設計概念**：Use Case Service、Command/Query 分離（類 CQRS）、DTO Mapping

---

## 🧱 專案架構

### 專案結構（樹狀圖）

```text
POSv01/
├─ Domain/
│  └─ Entities/
│     ├─ Product.cs
│     ├─ Sale.cs / SaleItem.cs
│     ├─ Return.cs / ReturnItem.cs
│     ├─ Inventory.cs
│     └─ PaymentMethod.cs
│
├─ Infrastructure/
│  ├─ PosDbContext.cs
│  ├─ DbPathProvider.cs
│  ├─ SchemaRepair.cs
│  └─ (EF Core Migrations)
│
├─ Services/
│  ├─ CheckoutService.cs
│  ├─ ReturnService.cs
│  ├─ SaleQueryService.cs
│  └─ ReturnQueryService.cs
│
├─ UI/
│  ├─ MainForm.cs
│  ├─ ProductsForm.cs
│  ├─ OrderLookupForm.cs
│  ├─ ReturnForm.cs
│  └─ ReceiptPreviewForm.cs
│
└─ ViewModels/
   └─ CartItemViewModel.cs
```

### 分層流程（UI → 後端服務 → Domain → DB）

```text
WinForms UI（View）
   ↓ 觸發流程
Services（Use Case / Query）
   ↓ 使用
Domain Entities（Sale / Return / Inventory）
   ↓ 寫入與查詢
Infrastructure（EF Core + SQLite）
```

---

## 🧾 功能清單

### 核心收銀功能
- 條碼輸入 → 購物車 DataGridView 即時更新
- 支援付款方式：現金 / 信用卡 / 行動支付（模擬完成）
- 收據預覽（列印流程的前置）

### 訂單與退貨
- 訂單查詢：日期 / 單號 / 關鍵字
- **部分退貨**：可退回部分品項與數量
- 退貨查詢 + 退貨收據預覽

### 工程與穩定性
- Migration 優先 DB 初始化 + fallback 建表
- Design-time safe initialization（設計工具不初始化 DB）
- 事件綁定防重複，避免按鈕觸發兩次

---

## 🔄 核心流程（Business Workflows）

### 結帳流程
```text
掃描/加入商品 → 購物車
       ↓
選擇付款方式（現金/卡/行動支付）
       ↓
CheckoutService（交易一致性 + 寫入資料）
       ↓
更新庫存 + 產生收據
```

### 退貨流程（支援部分退貨）
```text
輸入原單號 → 選擇退貨品項與數量
       ↓
ReturnService 驗證退貨規則
       ↓
寫入退貨資料 + 回補庫存
       ↓
退貨收據預覽
```

---

## 🧠 後端工程設計重點

### 1) 交易一致性（Atomic Write）
結帳/退貨流程採用 **Transaction + Rollback** 確保：
- 任一步驟失敗 → 整筆資料回滾
- 避免「有交易、沒扣庫存」或「扣庫存、沒交易」的錯誤狀態

> 這是 POS / 金流 / 庫存系統的核心後端需求。

### 2) 報表查詢效能（Query Optimization）
查詢層（Query Services）具備：
- `AsNoTracking()` 讀取效能優化
- 條件篩選（日期 / 單號 / 關鍵字）
- DTO 投影（只取必要欄位）
- 分頁/限制筆數（避免大查詢）

### 3) DB 完整性與效能
- `SaleNo` 唯一（Unique）
- `CreatedAt` 建立索引（Index）

---

## 🧩 代表性程式碼亮點（Readable Snippets）

### ✅ 設計模式安全初始化（避免 WinForms Designer 崩潰）
```csharp
public MainForm()
{
    InitializeComponent();
    ApplyConvenienceStoreTheme();

    // ✅ 設計工具打開時不初始化 DB（避免 Designer 報錯）
    if (IsDesignTime()) return;

    InitRuntime();
}

private static bool IsDesignTime()
{
    return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
}
```

### ✅ 事件只綁一次（修正 Click 觸發兩次）
```csharp
private static void HookClickOnce(Button btn, EventHandler handler)
{
    for (int i = 0; i < 8; i++)
        btn.Click -= handler;

    btn.Click += handler;
}
```

### ✅ 結帳流程入口（UI 觸發 Use Case Service）
```csharp
var sale = _checkoutService.Checkout(_cartItems.ToList(), method, paidAmount);

// 使用 QueryService 組收據 DTO
var receipt = _saleQueryService.GetReceipt(sale.Id);
using (var preview = new ReceiptPreviewForm(receipt))
{
    preview.ShowDialog(this);
}
```

---

## 🗄️ 資料庫初始化（Migration → fallback → 修復）

SQLite + EF Core

啟動時流程：
1) 優先套用 Migration  
2) 失敗則 fallback `EnsureCreated()`  
3) 可選擇 SchemaRepair 進行欄位修復

```csharp
try { _dbContext.Database.Migrate(); }
catch { _dbContext.Database.EnsureCreated(); }

SchemaRepair.EnsureProductsHasImagePath(_dbContext);
```

---

## ▶️ 快速開始（Quick Start）

### 環境需求
- Visual Studio 2022+
- .NET SDK（建議 6+）
- Windows（WinForms）

### 執行方式
1. 下載/Clone 專案
2. 使用 Visual Studio 開啟 `.sln`
3. 按 **F5** 執行

---

## 🔧 未來移植到 ASP.NET Core Web API 的方向（後端延伸）

本專案已將商業邏輯集中於 Service 層，因此移植為 Web API 時只需增加 Controller：

- `POST /api/checkout` → `CheckoutService.Checkout()`
- `POST /api/returns` → `ReturnService.CreateReturn()`
- `GET /api/sales` → `SaleQueryService.SearchSales(...)`
- `GET /api/returns` → `ReturnQueryService.SearchReturns(...)`

## 🎯 技能對應（Skills Mapping）

| 專案亮點 | 使用技術 | 對應後端能力 |
|---|---|---|
| 結帳/退貨 Transaction + Rollback | EF Core | 資料一致性、交易安全 |
| Use Case Service 流程封裝 | C# OOP | 架構分層、可維護性 |
| 部分退貨 + 商業規則驗證 | Service Layer | 商業邏輯、防呆驗證 |
| 查詢最佳化（報表/歷史） | LINQ + EF Core | 查詢效能、DTO 投影 |
| SaleNo Unique + CreatedAt Index | SQLite | DB 完整性/效能 |
| WinForms 設計工具穩定化 | WinForms | 實戰除錯與穩定性 |

---

## 🧭 Roadmap（待優化/可擴充）
- [ ] 補上「庫存不足驗證」（可在結帳或加入購物車階段檢核）
- [ ] 建立 `InventoryTxn` 庫存異動紀錄（sale / return / adjust）
- [ ] 增加 Unit Tests（退貨規則、現金不足、購物車空）
- [ ] 增加 async 版本（SaveChangesAsync）更接近 Web API 實務

---

## 👤 作者
- 姓名：黃泰銘
- LinkedIn：（你的連結）
- Email：fuknwin@gmail.com
