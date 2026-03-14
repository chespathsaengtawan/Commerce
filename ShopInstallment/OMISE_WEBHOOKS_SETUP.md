# การตั้งค่า Omise Webhooks

## 📋 ขั้นตอนการตั้งค่า Webhooks ใน Omise Dashboard

### 1. เข้าสู่ Omise Dashboard
- ไปที่ https://dashboard.omise.co/
- เข้าสู่ระบบด้วยบัญชีของคุณ

### 2. เปิดหน้า Webhooks Settings
- คลิกที่เมนู **Settings** (⚙️) ด้านซ้ายบน
- เลือก **Webhooks**

### 3. สร้าง Webhook Endpoint ใหม่
- คลิกปุ่ม **+ Create a webhook**
- กรอกข้อมูลดังนี้:

**URL:**
```
https://yourdomain.com/api/webhooks/omise
```
หรือถ้าทดสอบ local ใช้ ngrok:
```
https://your-ngrok-id.ngrok.io/api/webhooks/omise
```

**Description:** (ตั้งชื่อให้จำง่าย)
```
ShopInstallment Production Webhook
```

**Events to subscribe:** เลือก events ที่ต้องการ
- ✅ `charge.complete` - เมื่อการชำระเงินสำเร็จ (สำคัญที่สุด)
- ✅ `charge.create` - เมื่อมีการสร้างชาร์จ
- ✅ `charge.update` - เมื่อสถานะชาร์จเปลี่ยน
- ✅ `charge.capture` - เมื่อมีการดึงเงิน
- ✅ `refund.create` - เมื่อมีการคืนเงิน
- ⚪ `transfer.create` - เมื่อมีการโอนเงินให้ผู้ขาย (ถ้าใช้ Marketplace)
- ⚪ `transfer.update` - เมื่อสถานะการโอนเปลี่ยน

### 4. รับ Webhook Secret
- หลังจากสร้าง Webhook สำเร็จ จะได้ **Signing Secret**
- คัดลอก secret นี้

### 5. เพิ่ม Webhook Secret ใน User Secrets

เปิด Terminal และรันคำสั่ง:

```powershell
# Production
dotnet user-secrets set "Omise:WebhookSecret" "YOUR-WEBHOOK-SECRET-HERE"

# หรือแก้ไข appsettings.json (ไม่แนะนำสำหรับ production)
```

### 6. ตั้งค่าเพิ่มเติม
- **Environment:** เลือก Test หรือ Live
- **Status:** ตรวจสอบให้เป็น **Active** ✅
- **API Version:** ใช้เวอร์ชันล่าสุด (2019-05-29 หรือใหม่กว่า)

---

## 🧪 การทดสอบ Webhook (ด้วย ngrok)

### 1. ติดตั้ง ngrok
```powershell
# ดาวน์โหลดจาก https://ngrok.com/download
# หรือใช้ chocolatey
choco install ngrok
```

### 2. เปิด Tunnel ไปยัง localhost
```powershell
# เปิด terminal ใหม่
ngrok http 5000

# หรือถ้า app รันที่ port อื่น
ngrok http 7123
```

### 3. คัดลอก URL จาก ngrok
จะได้ URL แบบนี้:
```
https://abc123.ngrok.io -> http://localhost:5000
```

### 4. อัปเดต Webhook URL ใน Omise
ใช้ URL:
```
https://abc123.ngrok.io/api/webhooks/omise
```

### 5. ทดสอบส่ง Webhook จาก Dashboard
- ไปที่ Webhooks settings
- คลิก **Test** ข้างๆ webhook endpoint
- เลือก event type: `charge.complete`
- คลิก **Send test event**

### 6. ตรวจสอบ Response
- ดู log ใน VS Code terminal
- Response ควรเป็น `200 OK`

---

## 🔒 Webhook Security

### 1. การตรวจสอบ Signature
Webhook controller ตรวจสอบ signature อัตโนมัติ:
- Header: `X-Omise-Signature`
- Algorithm: HMAC-SHA256
- Secret: ใช้ค่าจาก config

### 2. Best Practices
- ✅ เก็บ Webhook Secret ใน User Secrets หรือ Environment Variables
- ✅ ใช้ HTTPS สำหรับ production
- ✅ บันทึก log ทุก webhook event
- ✅ ตรวจสอบ signature ทุกครั้ง
- ❌ อย่าเปิดเผย webhook secret ใน code

---

## 📊 Events ที่รองรับ

| Event | Description | Action |
|-------|-------------|--------|
| `charge.complete` | ชำระเงินสำเร็จ | อัปเดต Payment เป็น Completed, อัปเดต Order |
| `charge.create` | สร้างชาร์จ | บันทึก log |
| `charge.update` | สถานะชาร์จเปลี่ยน | อัปเดตสถานะ (failed, expired, successful) |
| `charge.capture` | ดึงเงิน | เหมือน charge.complete |
| `refund.create` | คืนเงิน | อัปเดต Payment เป็น Refunded, อัปเดต Order |
| `transfer.create` | โอนเงินให้ผู้ขาย | บันทึก log การโอน |
| `transfer.update` | สถานะการโอนเปลี่ยน | อัปเดตสถานะการโอน |

---

## 🐛 Troubleshooting

### Webhook ไม่ได้รับ
1. ตรวจสอบ URL ถูกต้อง
2. ตรวจสอบ app กำลังรันอยู่
3. ตรวจสอบ firewall ไม่บล็อก
4. ใช้ ngrok สำหรับ local testing

### Signature ไม่ตรงกัน
1. ตรวจสอบ Webhook Secret ถูกต้อง
2. ตรวจสอบ request body ไม่ถูก modify

### Payment ไม่อัปเดต
1. ดู log ใน console
2. ตรวจสอบ TransactionId ตรงกัน
3. ตรวจสอบ database connection

---

## 📝 ตัวอย่าง Webhook Payload

### charge.complete
```json
{
  "key": "charge.complete",
  "data": {
    "id": "chrg_test_123456",
    "object": "charge",
    "status": "successful",
    "amount": 100000,
    "currency": "thb",
    "metadata": {
      "orderId": "123"
    }
  }
}
```

### refund.create
```json
{
  "key": "refund.create",
  "data": {
    "id": "rfnd_test_123456",
    "object": "refund",
    "amount": 50000,
    "charge": "chrg_test_123456"
  }
}
```

---

## 🚀 Production Checklist

- [ ] ตั้งค่า Webhook URL เป็น production domain (HTTPS)
- [ ] เพิ่ม Webhook Secret ใน Environment Variables
- [ ] เลือก Live environment ใน Omise Dashboard
- [ ] Subscribe events ที่จำเป็น
- [ ] ทดสอบ webhook ด้วย test events
- [ ] ตั้งค่า monitoring และ alerting
- [ ] เตรียม error handling และ retry logic
- [ ] บันทึก log ทุก webhook event

---

## 📞 การติดต่อ Omise Support

- 📧 Email: support@omise.co
- 📚 Docs: https://www.omise.co/docs
- 💬 Dashboard: https://dashboard.omise.co/
