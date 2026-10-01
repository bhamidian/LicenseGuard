# دیاگرام جریان‌های لایسنس

این سند جریان‌های فعلی صدور، تمدید، فعال‌سازی و اعتبارسنجی لایسنس را از روی کد موجود نشان می‌دهد. گام‌های پیاده‌سازی‌شده با سبز، گام‌های پیشنهادی با زرد و مانع‌های فعلی با قرمز مشخص شده‌اند.

## راهنمای وضعیت

- **پیاده‌سازی‌شده:** مسیر یا قاعده در کد وجود دارد.
- **نیازمند تصمیم/تکمیل:** برای کامل شدن رفتار محصول یا دسترسی عملی به مسیر، کار دیگری لازم است.
- **مانع فعلی:** بر اساس کد فعلی مسیر بعدی به نتیجه‌ی موفق نمی‌رسد.

## صدور لایسنس

```mermaid
flowchart TD
    A[POST /api/licenses] --> B[LicenseController]
    B --> C[Dispatch CreateLicenseCommand with MediatR]
    C --> D{FluentValidation passes?}
    D -- No --> D1[Return validation errors]
    D -- Yes --> E[Begin Serializable transaction]
    E --> F[Load Subscription with FOR UPDATE<br/>Include Plan and Features]
    F --> G{Subscription exists, is not canceled or expired, and has no current License?}
    G -- No --> G1[Return NotFound or Conflict<br/>Dispose transaction without commit]
    G -- Yes --> H{Requested Features belong to the Plan?}
    H -- No --> H1[Return validation error<br/>Dispose transaction without commit]
    H -- Yes --> I[LicenseService requests a key from the Provider]
    I --> J[CSPRNG generates 32 random bytes<br/>Encode as 64 hexadecimal characters]
    J --> K[Create CreateLicenseRecord]
    K --> L[ILicenseRepository.Create<br/>Build License and snapshot Features and Limits]
    L --> M[Initial status: PENDING]
    M -. Planned, not implemented .-> W[Admin approval command<br/>Authorize Admin]
    W -.-> X[Change status, add StatusHistory and AuditLog<br/>Then allow device activation]
    M --> N[License.GetSigningData<br/>Snapshot Customer, Product, Plan, dates, Features and Limits]
    N --> O[Canonicalize and sign with RSA-PSS/SHA-256<br/>Use Private Key]
    O --> P[Set Signature on License]
    P --> Q[Validate CreateAuditLogRecord with AuditLogService]
    Q --> R{Audit record valid?}
    R -- No --> R1[Return error<br/>Dispose transaction without commit]
    R -- Yes --> S[Create AuditLog through its Repository]
    S --> T[UnitOfWork.SaveChanges]
    T --> U[Commit transaction]
    U --> V[Return LicenseId, Key, Signature, Product, Plan and validity dates]

    classDef done fill:#e5f4e8,stroke:#31834b,color:#102b17;
    classDef todo fill:#fff4cc,stroke:#b38b00,color:#403300;
    classDef blocker fill:#ffe1e1,stroke:#bc3030,color:#421010;
    class A,B,C,D,E,F,H,I,J,K,L,N,O,P,Q,S,T,U,V done;
    class M blocker;
    class W,X todo;
```

### نکته‌ی مهم: گسست بین صدور و فعال‌سازی

صدور، لایسنس را با وضعیت `PENDING` ذخیره می‌کند. در مسیر فعلی Handler فعال‌سازی، قبل از ایجاد Activation، `License.IsValidAt` بررسی می‌شود؛ این تابع فقط برای وضعیت `ACTIVE` مقدار درست می‌دهد. در Application و API فعلی، Command/Endpoint مدیریتی برای انتقال `PENDING` به `ACTIVE` پیدا نشد. متد `License.Activate(...)` فقط Activation دستگاه را اضافه می‌کند و وضعیت خود لایسنس را از `PENDING` به `ACTIVE` تغییر نمی‌دهد. همچنین endpoint صدور از `IssuedByAdminId` ارسالی در Command استفاده می‌کند، ولی بررسی نقش/مجوز Admin در Controller یا Handler فعلی دیده نشد.

در نتیجه، **لایسنس تازه‌صادرشده در وضعیت فعلی نمی‌تواند از مسیر API فعال‌سازی شود** تا وقتی مسیر تغییر وضعیت اضافه شود. باید تصمیم گرفته شود که صدور مستقیماً لایسنس را فعال کند یا یک تأیید مدیریتی جداگانه لازم باشد. پیشنهاد، تعریف Command مدیریتی برای فعال‌سازی/تأیید لایسنس است که مجوز Admin، تغییر وضعیت، ثبت StatusHistory و AuditLog و ذخیره‌سازی اتمیک را انجام دهد.

## تمدید Subscription و امضای مجدد License

```mermaid
flowchart TD
    A[POST /api/subscriptions/{id}/renew] --> B[SubscriptionController]
    B --> C[Create RenewSubscriptionCommand]
    C --> D{FluentValidation passes?}
    D -- No --> D1[Return validation errors]
    D -- Yes --> E[Begin Serializable transaction]
    E --> F[Load Subscription with FOR UPDATE<br/>Include current License and snapshots]
    F --> G{Subscription found?}
    G -- No --> G1[Return NotFound<br/>Dispose transaction without commit]
    G -- Yes --> H[Create SubscriptionRenewal from record<br/>Renew Subscription in Domain]
    H --> I[Publish SubscriptionRenewedNotification<br/>In-process through MediatR]
    I --> J[RenewLicenseAfterSubscriptionRenewalHandler]
    J --> K{Current License exists?}
    K -- No --> K1[Leave License unchanged]
    K -- Yes --> L[Update License expiration and renewal history]
    L --> M[Re-sign LicenseSigningData with RSA-PSS/SHA-256]
    M --> N[SaveChanges and commit in the same transaction]
    K1 --> N
    N --> O[Return Subscription renewal and current License details]

    classDef done fill:#e5f4e8,stroke:#31834b,color:#102b17;
    classDef todo fill:#fff4cc,stroke:#b38b00,color:#403300;
    class A,B,C,D,E,F,H,I,J,K,L,M,N,O done;
    class K1 todo;
```

## فعال‌سازی دستگاه

```mermaid
flowchart TD
    A[POST /api/licenses/activate] --> B[LicenseController]
    B --> C[Create ActivateLicenseCommand and validate]
    C --> D[Begin Serializable transaction]
    D --> E[Load License with FOR UPDATE<br/>Include Subscription, Activations, Features and Limits]
    E --> F{License and Signature found?}
    F -- No --> F1[Return invalid License or NotFound]
    F -- Yes --> G{Signature is valid with Public Key?}
    G -- No --> G1[Return Conflict: invalid Signature]
    G -- Yes --> H{License is currently valid and ACTIVE?}
    H -- No --> H1[Return Conflict: outside validity period or inactive<br/>PENDING is rejected here]
    H -- Yes --> I{Activation limit reached or Machine/Instance already active?}
    I -- Yes --> I1[Return Conflict]
    I -- No --> J[Create LicenseActivation from record]
    J --> K[Attach Activation to License in Domain]
    K --> L[SaveChanges and commit]
    L --> M[Return ActivationId and License snapshot]

    classDef done fill:#e5f4e8,stroke:#31834b,color:#102b17;
    classDef blocker fill:#ffe1e1,stroke:#bc3030,color:#421010;
    class A,B,C,D,E,F,G,I,J,K,L,M done;
    class H blocker;
```

## اعتبارسنجی در زمان استفاده

```mermaid
flowchart TD
    A[POST /api/licenses/validate] --> B[LicenseController]
    B --> C[Create ValidateLicenseCommand and validate]
    C --> D[Load License with Subscription and Activations]
    D --> E{License and Signature exist and verify?}
    E -- No --> E1[Return IsValid=false]
    E -- Yes --> F{Validity period, License status and Subscription are valid?}
    F -- No --> F1[Return IsValid=false]
    F -- Yes --> G{Active Activation matches MachineId and InstanceId?}
    G -- No --> G1[Return IsValid=false]
    G -- Yes --> H[Update LastValidatedAt]
    H --> I[SaveChanges]
    I --> J[Return IsValid=true with LicenseSigningData and Signature]

    classDef done fill:#e5f4e8,stroke:#31834b,color:#102b17;
    class A,B,C,D,E,F,G,H,I,J done;
```

## وضعیت عملیات مدیریتی و موارد پیشنهادی بعدی

| مورد | وضعیت فعلی در کد | گام پیشنهادی |
|---|---|---|
| صدور و امضای License | پیاده‌سازی شده؛ وضعیت اولیه `PENDING` | مشخص‌کردن سیاست فعال‌شدن و افزودن مسیر انتقال به `ACTIVE` |
| فعال‌سازی دستگاه و محدودیت Instance/Device | Handler و تراکنش Serializable پیاده‌سازی شده | بعد از رفع گسست وضعیت، تست همزمانی با دیتابیس واقعی |
| تمدید Subscription و License جاری | Notification درون‌برنامه‌ای و امضای مجدد پیاده‌سازی شده | تست rollback و سناریوی License بدون امضای قبلی؛ Outbox فقط در صورت نیاز به پردازش بیرونی/پایدار |
| اعتبارسنجی License | امضا، زمان، وضعیت و Activation بررسی می‌شود | تعریف رفتار و کد پاسخ برای حالت‌های ردشده و تست یکپارچه |
| Suspend / Resume / Revoke | متدهای Domain وجود دارد | افزودن Command، Validator، مجوز Admin، Controller و AuditLog |
| Expire | قاعده‌ی Domain و ردشدن پس از تاریخ انقضا وجود دارد | Worker/Job برای تغییر وضعیت ذخیره‌شده به `EXPIRED` و ثبت تاریخچه، اگر گزارش وضعیت صریح لازم است |
| مجوز دسترسی API | در Controllerهای فعلی `[Authorize]` دیده نشد | محافظت از عملیات مدیریتی و استخراج شناسه Admin از هویت احراز‌شده به‌جای اعتماد به ورودی درخواست |
| آزمون جریان‌ها | تست‌های Domain وجود دارند | تست Application/Infrastructure برای صدور، کلید/امضا، تمدید، رقابت و rollback |

## فایل‌های مرجع اصلی

- صدور: `03-Application/LicenseGuard.Application/Features/License/Commands/CreateLicensesCommand/CreateLicenseCommandHandler.cs`
- فعال‌سازی: `03-Application/LicenseGuard.Application/Features/License/Commands/ActivateLicense/ActivateLicenseCommandHandler.cs`
- اعتبارسنجی: `03-Application/LicenseGuard.Application/Features/License/Commands/ValidateLicense/ValidateLicenseCommandHandler.cs`
- تمدید: `03-Application/LicenseGuard.Application/Features/Subscription/Commands/RenewSubscriptionCommandHandler.cs`
- رویداد تمدید License: `03-Application/LicenseGuard.Application/Features/Subscription/Events/RenewLicenseAfterSubscriptionRenewalHandler.cs`
- قوانین وضعیت و اعتبار License: `01-Domain/LicenseGuard.Domain/Entities/License.cs`
- تولید کلید و امضا: `02-Infrastructure/LicenseGuard.Infrstructure.SecurityService/Services/`


## جریان مدیریتی وضعیت License؛ کار پیشنهادی

متدهای Domain برای `Suspend`، `Resume`، `Revoke` و `Expire` وجود دارند، اما در Application و API فعلی مسیرهای Command/Handler/Controller برای فراخوانی آن‌ها پیدا نشد. این دیاگرام، جریان پیشنهادی آینده را نشان می‌دهد و مسیر پیاده‌سازی‌شده نیست.

```mermaid
flowchart LR
    A[Admin requests a status change] --> B[Planned: authenticate and authorize Admin]
    B --> C[Planned: Command and FluentValidation]
    C --> D[Planned: load License and begin transaction]
    D --> E{Requested operation}
    E -->|Approve issuance| F[Planned Domain method: PENDING to ACTIVE]
    E -->|Suspend| G[License.Suspend]
    E -->|Resume| H[License.Resume]
    E -->|Revoke| I[License.Revoke]
    E -->|Record expiration| J[License.Expire]
    F --> K[Planned: add StatusHistory and AuditLog]
    G --> K
    H --> K
    I --> K
    J --> K
    K --> L[Planned: SaveChanges and commit]
    L --> M[Return Admin API response]

    classDef todo fill:#fff4cc,stroke:#b38b00,color:#403300;
    class A,B,C,D,E,F,G,H,I,J,K,L,M todo;
```
