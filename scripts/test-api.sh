#!/bin/bash
# Airport Lounge – Full API Integration Test Script
# Prerequisites: docker compose up -d (API + PostgreSQL running)
# Usage: ./scripts/test-api.sh [http://localhost:8080]

BASE_URL="${1:-http://localhost:8080}"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
CYAN='\033[0;36m'
NC='\033[0m'

PASS=0
FAIL=0

pass()    { echo -e "${GREEN}PASS${NC}: $1"; PASS=$((PASS+1)); }
fail()    { echo -e "${RED}FAIL${NC}: $1"; FAIL=$((FAIL+1)); }
section() { echo ""; echo -e "${CYAN}══════ $1 ══════${NC}"; }

# Extract JSON field helper
json_field() { echo "$1" | grep -o "\"$2\":\"[^\"]*\"" | head -1 | cut -d'"' -f4; }
json_num()   { echo "$1" | grep -o "\"$2\":[0-9]*" | head -1 | cut -d':' -f2; }

http_test() {
  local label="$1"
  local resp="$2"
  local expected="$3"
  local http=$(echo "$resp" | tail -n1)
  local body=$(echo "$resp" | sed '$d')
  if [[ "$http" == "$expected" ]]; then
    pass "$label → $http"
    echo "$body"
  else
    fail "$label → expected $expected, got $http | $body"
  fi
  echo "$body"
}

# ─── SETUP ───────────────────────────────────────────────────────────────────
RAND=$(date +%s)
MGR_EMAIL="manager${RAND}@test.com"
STAFF_EMAIL="staff${RAND}@test.com"
MGR_CODE="MGR${RAND}"
STAFF_CODE="STF${RAND}"

echo ""
echo -e "${CYAN}═══════════════════════════════════════════════${NC}"
echo -e "${CYAN}   Airport Lounge – Full API Integration Tests   ${NC}"
echo -e "${CYAN}   Base URL: $BASE_URL                           ${NC}"
echo -e "${CYAN}═══════════════════════════════════════════════${NC}"

# ─── 1. HEALTH ───────────────────────────────────────────────────────────────
section "1. Health Check"
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/health")
[[ "$HTTP" == "200" ]] && pass "GET /health → 200" || fail "GET /health → $HTTP"

# ─── 2. AUTH ─────────────────────────────────────────────────────────────────
section "2. Auth"

# 2a. Admin login (email)
RESP=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"emailOrPhone":"admin@airportlounge.com","password":"Admin@123"}')
if echo "$RESP" | grep -q "accessToken"; then
  pass "POST /api/auth/login (admin by email) → 200"
  ADMIN_TOKEN=$(json_field "$RESP" "accessToken")
  REFRESH_TOKEN=$(json_field "$RESP" "refreshToken")
else
  fail "Admin login failed: $RESP"; exit 1
fi

# 2b. Invalid credentials (correct format, wrong values – password >= 6 chars required by validator)
HTTP=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"emailOrPhone":"nobody@test.com","password":"wrongpwd"}')
[[ "$HTTP" == "401" ]] && pass "POST /api/auth/login (invalid creds) → 401" || fail "Expected 401, got $HTTP"

# 2c. Refresh token (RefreshTokenCommand takes accessToken + refreshToken)
RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d "{\"accessToken\":\"$ADMIN_TOKEN\",\"refreshToken\":\"$REFRESH_TOKEN\"}")
HTTP=$(echo "$RESP" | tail -n1)
[[ "$HTTP" == "200" ]] && pass "POST /api/auth/refresh → 200" || fail "POST /api/auth/refresh → $HTTP"

# ─── 3. EMPLOYEES ────────────────────────────────────────────────────────────
section "3. Employees"

# 3a. Create Manager
RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/employees" \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d "{\"employeeCode\":\"$MGR_CODE\",\"fullName\":\"Test Manager\",
      \"email\":\"$MGR_EMAIL\",\"phoneNumber\":\"0901234567\",
      \"password\":\"Manager@123\",\"role\":1,
      \"department\":\"Operations\",\"position\":\"Lounge Manager\",\"skills\":\"Management\"}")
HTTP=$(echo "$RESP" | tail -n1)
BODY=$(echo "$RESP" | sed '$d')
[[ "$HTTP" == "201" ]] && pass "POST /api/employees (Manager) → 201" || fail "POST /api/employees (Manager) → $HTTP | $BODY"
MGR_EMP_ID=$(echo "$BODY" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)

# 3b. Create Staff
RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/employees" \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d "{\"employeeCode\":\"$STAFF_CODE\",\"fullName\":\"Test Staff\",
      \"email\":\"$STAFF_EMAIL\",\"phoneNumber\":\"0907654321\",
      \"password\":\"Staff@123\",\"role\":0,
      \"department\":\"Service\",\"position\":\"Lounge Staff\",\"skills\":\"VIP Service\"}")
HTTP=$(echo "$RESP" | tail -n1)
BODY=$(echo "$RESP" | sed '$d')
[[ "$HTTP" == "201" ]] && pass "POST /api/employees (Staff) → 201" || fail "POST /api/employees (Staff) → $HTTP | $BODY"
STAFF_EMP_ID=$(echo "$BODY" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)

# 3c. List employees
RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/employees?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer $ADMIN_TOKEN")
HTTP=$(echo "$RESP" | tail -n1)
[[ "$HTTP" == "200" ]] && pass "GET /api/employees → 200" || fail "GET /api/employees → $HTTP"

# 3d. Get employee by ID
if [[ -n "$MGR_EMP_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/employees/$MGR_EMP_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "GET /api/employees/{id} → 200" || fail "GET /api/employees/{id} → $HTTP"
else
  echo -e "${YELLOW}SKIP${NC}: GET /api/employees/{id} (no employee ID)"
fi

# 3e. Update employee
if [[ -n "$MGR_EMP_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X PUT "$BASE_URL/api/employees/$MGR_EMP_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"employeeId\":\"$MGR_EMP_ID\",\"fullName\":\"Updated Manager\",
        \"phoneNumber\":\"0909999999\",\"department\":\"Operations\",
        \"position\":\"Senior Manager\",\"skills\":\"Leadership\",\"address\":\"123 Airport Rd\"}")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "PUT /api/employees/{id} → 200" || fail "PUT /api/employees/{id} → $HTTP | $(echo "$RESP" | sed '$d')"
fi

# ─── 4. LOGIN (Manager + Phone) ───────────────────────────────────────────────
section "4. Manager & Phone Login"

RESP=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"emailOrPhone\":\"$MGR_EMAIL\",\"password\":\"Manager@123\"}")
if echo "$RESP" | grep -q "accessToken"; then
  pass "POST /api/auth/login (manager by email) → 200"
  MGR_TOKEN=$(json_field "$RESP" "accessToken")
else
  fail "Manager login failed: $RESP"; MGR_TOKEN="$ADMIN_TOKEN"
fi

RESP=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"emailOrPhone\":\"$STAFF_EMAIL\",\"password\":\"Staff@123\"}")
if echo "$RESP" | grep -q "accessToken"; then
  pass "POST /api/auth/login (staff by email) → 200"
  STAFF_TOKEN=$(json_field "$RESP" "accessToken")
else
  fail "Staff login failed: $RESP"; STAFF_TOKEN=""
fi

# Phone number login
RESP=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"emailOrPhone":"0907654321","password":"Staff@123"}')
if echo "$RESP" | grep -q "accessToken"; then
  pass "POST /api/auth/login (staff by phone) → 200"
else
  fail "Staff phone login failed: $RESP"
fi

# ─── 5. DASHBOARD ─────────────────────────────────────────────────────────────
section "5. Dashboard"

HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/dashboard/admin" \
  -H "Authorization: Bearer $ADMIN_TOKEN")
[[ "$HTTP" == "200" ]] && pass "GET /api/dashboard/admin → 200" || fail "GET /api/dashboard/admin → $HTTP"

HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/dashboard/manager" \
  -H "Authorization: Bearer $MGR_TOKEN")
[[ "$HTTP" == "200" ]] && pass "GET /api/dashboard/manager → 200" || fail "GET /api/dashboard/manager → $HTTP"

if [[ -n "$STAFF_EMP_ID" ]]; then
  HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/dashboard/staff/$STAFF_EMP_ID" \
    -H "Authorization: Bearer ${STAFF_TOKEN:-$ADMIN_TOKEN}")
  [[ "$HTTP" == "200" ]] && pass "GET /api/dashboard/staff/{id} → 200" || fail "GET /api/dashboard/staff/{id} → $HTTP"
fi

# Auth: Staff cannot access Admin dashboard
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/dashboard/admin" \
  -H "Authorization: Bearer ${STAFF_TOKEN:-}")
[[ "$HTTP" == "403" ]] && pass "Staff → /api/dashboard/admin correctly returns 403" || fail "Expected 403, got $HTTP"

# ─── 6. ZONES ────────────────────────────────────────────────────────────────
section "6. Zones"

# 6a. Create zone
RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/zones" \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"VIP Zone A","description":"Main VIP area","capacity":50}')
HTTP=$(echo "$RESP" | tail -n1)
BODY=$(echo "$RESP" | sed '$d')
[[ "$HTTP" == "200" ]] && pass "POST /api/zones → 200" || fail "POST /api/zones → $HTTP | $BODY"
ZONE_ID=$(echo "$BODY" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)

# 6b. Get all zones
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/zones" \
  -H "Authorization: Bearer $ADMIN_TOKEN")
[[ "$HTTP" == "200" ]] && pass "GET /api/zones → 200" || fail "GET /api/zones → $HTTP"

# 6c. Filter zones by status
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/zones?status=0" \
  -H "Authorization: Bearer $ADMIN_TOKEN")
[[ "$HTTP" == "200" ]] && pass "GET /api/zones?status=Available → 200" || fail "GET /api/zones?status=0 → $HTTP"

# 6d. Update zone status (NeedsSupport)
if [[ -n "$ZONE_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X PUT "$BASE_URL/api/zones/$ZONE_ID/status" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"zoneId\":\"$ZONE_ID\",\"newStatus\":3,\"notes\":\"Needs extra help\"}")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "PUT /api/zones/{id}/status → 200" || fail "PUT /api/zones/{id}/status → $HTTP | $(echo "$RESP" | sed '$d')"
fi

# 6e. Zone alerts
RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/zones/alerts" \
  -H "Authorization: Bearer $MGR_TOKEN")
HTTP=$(echo "$RESP" | tail -n1)
BODY=$(echo "$RESP" | sed '$d')
[[ "$HTTP" == "200" ]] && pass "GET /api/zones/alerts → 200 ($(echo "$BODY" | grep -o '"alertType"' | wc -l | tr -d ' ') alerts found)" || fail "GET /api/zones/alerts → $HTTP"

# ─── 7. SHIFTS ───────────────────────────────────────────────────────────────
section "7. Shifts"

# 7a. Create shift
RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/shifts" \
  -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Morning Shift","startTime":"06:00:00","endTime":"14:00:00","description":"Morning lounge shift"}')
HTTP=$(echo "$RESP" | tail -n1)
BODY=$(echo "$RESP" | sed '$d')
[[ "$HTTP" == "200" ]] && pass "POST /api/shifts → 200" || fail "POST /api/shifts → $HTTP | $BODY"
SHIFT_ID=$(echo "$BODY" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)

# 7b. Assign shift to staff
TODAY=$(date -u +%Y-%m-%d)
if [[ -n "$SHIFT_ID" && -n "$STAFF_EMP_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/shifts/assign" \
    -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
    -d "{\"shiftId\":\"$SHIFT_ID\",\"employeeId\":\"$STAFF_EMP_ID\",
        \"date\":\"${TODAY}T00:00:00Z\",\"loungeZoneId\":null}")
  HTTP=$(echo "$RESP" | tail -n1)
  BODY=$(echo "$RESP" | sed '$d')
  [[ "$HTTP" == "200" ]] && pass "POST /api/shifts/assign → 200" || fail "POST /api/shifts/assign → $HTTP | $BODY"

  # 7c. Get schedule
  NEXT_WEEK=$(date -u -v+7d +%Y-%m-%d 2>/dev/null || date -u -d "+7 days" +%Y-%m-%d 2>/dev/null || echo "$(date -u +%Y-%m-%d)")
  RESP=$(curl -s -w "\n%{http_code}" \
    "$BASE_URL/api/shifts/schedule?startDate=${TODAY}T00:00:00Z&endDate=${NEXT_WEEK}T23:59:59Z&employeeId=$STAFF_EMP_ID" \
    -H "Authorization: Bearer $MGR_TOKEN")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "GET /api/shifts/schedule → 200" || fail "GET /api/shifts/schedule → $HTTP"
fi

# 7d. Overlap check: assign same shift twice to same employee
if [[ -n "$SHIFT_ID" && -n "$STAFF_EMP_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/shifts/assign" \
    -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
    -d "{\"shiftId\":\"$SHIFT_ID\",\"employeeId\":\"$STAFF_EMP_ID\",
        \"date\":\"${TODAY}T00:00:00Z\",\"loungeZoneId\":null}")
  HTTP=$(echo "$RESP" | tail -n1)
  BODY=$(echo "$RESP" | sed '$d')
  [[ "$HTTP" == "400" ]] && pass "POST /api/shifts/assign (duplicate) → 400 (overlap prevented)" || fail "Expected 400 for overlap, got $HTTP | $BODY"
fi

# ─── 8. TASKS ────────────────────────────────────────────────────────────────
section "8. Tasks"

# 8a. Create task (unassigned)
RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/tasks" \
  -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
  -d '{"title":"Clean VIP area","description":"Deep clean after event","priority":2,
       "assignedToId":null,"loungeZoneId":null,"dueDate":null}')
HTTP=$(echo "$RESP" | tail -n1)
BODY=$(echo "$RESP" | sed '$d')
[[ "$HTTP" == "200" ]] && pass "POST /api/tasks (unassigned) → 200" || fail "POST /api/tasks → $HTTP | $BODY"
TASK_ID=$(echo "$BODY" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)

# 8b. Create task assigned to staff
if [[ -n "$STAFF_EMP_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/tasks" \
    -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
    -d "{\"title\":\"Serve VIP guests\",\"description\":\"Attend to VIP guests in zone A\",
         \"priority\":3,\"assignedToId\":\"$STAFF_EMP_ID\",
         \"loungeZoneId\":null,\"dueDate\":null}")
  HTTP=$(echo "$RESP" | tail -n1)
  BODY=$(echo "$RESP" | sed '$d')
  [[ "$HTTP" == "200" ]] && pass "POST /api/tasks (assigned) → 200" || fail "POST /api/tasks (assigned) → $HTTP | $BODY"
  TASK_ASSIGNED_ID=$(echo "$BODY" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)
fi

# 8c. Get tasks (paginated)
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/tasks?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer $MGR_TOKEN")
[[ "$HTTP" == "200" ]] && pass "GET /api/tasks → 200" || fail "GET /api/tasks → $HTTP"

# 8d. Filter tasks by status
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/tasks?status=0" \
  -H "Authorization: Bearer $MGR_TOKEN")
[[ "$HTTP" == "200" ]] && pass "GET /api/tasks?status=Pending → 200" || fail "GET /api/tasks?status=0 → $HTTP"

# 8e. Update task status → InProgress
if [[ -n "$TASK_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X PUT "$BASE_URL/api/tasks/$TASK_ID/status" \
    -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
    -d "{\"taskId\":\"$TASK_ID\",\"newStatus\":1}")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "PUT /api/tasks/{id}/status (InProgress) → 200" || fail "PUT /api/tasks/{id}/status → $HTTP | $(echo "$RESP" | sed '$d')"
fi

# 8f. Update task status → Completed
if [[ -n "$TASK_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X PUT "$BASE_URL/api/tasks/$TASK_ID/status" \
    -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
    -d "{\"taskId\":\"$TASK_ID\",\"newStatus\":2}")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "PUT /api/tasks/{id}/status (Completed) → 200" || fail "PUT /api/tasks/{id}/status → $HTTP | $(echo "$RESP" | sed '$d')"
fi

# 8g. Export tasks CSV
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/tasks/export?format=csv" \
  -H "Authorization: Bearer $MGR_TOKEN")
[[ "$HTTP" == "200" ]] && pass "GET /api/tasks/export?format=csv → 200" || fail "GET /api/tasks/export → $HTTP"

# ─── 9. NOTIFICATIONS ────────────────────────────────────────────────────────
section "9. Notifications"

# 9a. Send broadcast notification
RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/notifications" \
  -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
  -d '{"title":"Schedule Update","content":"Morning shift timings changed to 07:00-15:00",
       "type":1,"recipientIds":null,"relatedEntityType":null,"relatedEntityId":null}')
HTTP=$(echo "$RESP" | tail -n1)
[[ "$HTTP" == "200" ]] && pass "POST /api/notifications (broadcast) → 200" || fail "POST /api/notifications → $HTTP | $(echo "$RESP" | sed '$d')"

# 9b. Send to specific employee
if [[ -n "$STAFF_EMP_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/notifications" \
    -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
    -d "{\"title\":\"Your task is updated\",\"content\":\"Please check your assigned tasks\",
         \"type\":2,\"recipientIds\":[\"$STAFF_EMP_ID\"],
         \"relatedEntityType\":null,\"relatedEntityId\":null}")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "POST /api/notifications (to staff) → 200" || fail "POST /api/notifications (to staff) → $HTTP | $(echo "$RESP" | sed '$d')"
fi

# 9c. Get my notifications
if [[ -n "$STAFF_EMP_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" \
    "$BASE_URL/api/notifications/my/$STAFF_EMP_ID?unreadOnly=false&pageNumber=1&pageSize=10" \
    -H "Authorization: Bearer ${STAFF_TOKEN:-$ADMIN_TOKEN}")
  HTTP=$(echo "$RESP" | tail -n1)
  BODY=$(echo "$RESP" | sed '$d')
  [[ "$HTTP" == "200" ]] && pass "GET /api/notifications/my/{id} → 200" || fail "GET /api/notifications/my/{id} → $HTTP"

  # 9d. Mark notification as read
  NOTIF_ID=$(echo "$BODY" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
  if [[ -n "$NOTIF_ID" ]]; then
    RESP=$(curl -s -w "\n%{http_code}" -X PUT "$BASE_URL/api/notifications/$NOTIF_ID/read" \
      -H "Authorization: Bearer ${STAFF_TOKEN:-$ADMIN_TOKEN}")
    HTTP=$(echo "$RESP" | tail -n1)
    [[ "$HTTP" == "200" ]] && pass "PUT /api/notifications/{id}/read → 200" || fail "PUT /api/notifications/{id}/read → $HTTP"

    # 9e. Verify unread only returns 0 for this notification
    RESP=$(curl -s -w "\n%{http_code}" \
      "$BASE_URL/api/notifications/my/$STAFF_EMP_ID?unreadOnly=true&pageNumber=1&pageSize=10" \
      -H "Authorization: Bearer ${STAFF_TOKEN:-$ADMIN_TOKEN}")
    HTTP=$(echo "$RESP" | tail -n1)
    [[ "$HTTP" == "200" ]] && pass "GET /api/notifications/my/{id}?unreadOnly=true → 200" || fail "GET /api/notifications unread → $HTTP"
  fi
fi

# ─── 10. ATTENDANCE ──────────────────────────────────────────────────────────
section "10. Attendance"

# 10a. Manager edit (manual adjustment on test attendance record)
# Create a direct DB record via admin bypass:
# We test the report endpoint and edit/confirm APIs

START=$(date -u -v-7d +%Y-%m-%dT00:00:00Z 2>/dev/null || date -u -d "-7 days" +%Y-%m-%dT00:00:00Z 2>/dev/null || echo "${TODAY}T00:00:00Z")
END="${TODAY}T23:59:59Z"

RESP=$(curl -s -w "\n%{http_code}" \
  "$BASE_URL/api/attendance/report?startDate=$START&endDate=$END&pageNumber=1&pageSize=20" \
  -H "Authorization: Bearer $MGR_TOKEN")
HTTP=$(echo "$RESP" | tail -n1)
BODY=$(echo "$RESP" | sed '$d')
[[ "$HTTP" == "200" ]] && pass "GET /api/attendance/report → 200" || fail "GET /api/attendance/report → $HTTP"

# 10b. Export attendance CSV
HTTP=$(curl -s -o /dev/null -w "%{http_code}" \
  "$BASE_URL/api/attendance/export?startDate=$START&endDate=$END&format=csv" \
  -H "Authorization: Bearer $MGR_TOKEN")
[[ "$HTTP" == "200" ]] && pass "GET /api/attendance/export?format=csv → 200" || fail "GET /api/attendance/export → $HTTP"

# 10c. Export attendance PDF
HTTP=$(curl -s -o /dev/null -w "%{http_code}" \
  "$BASE_URL/api/attendance/export?startDate=$START&endDate=$END&format=pdf" \
  -H "Authorization: Bearer $MGR_TOKEN")
[[ "$HTTP" == "200" ]] && pass "GET /api/attendance/export?format=pdf → 200" || fail "GET /api/attendance/export (pdf) → $HTTP"

ATTENDANCE_ID=$(echo "$BODY" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
if [[ -n "$ATTENDANCE_ID" ]]; then
  # 10d. Manager update attendance record
  RESP=$(curl -s -w "\n%{http_code}" -X PUT "$BASE_URL/api/attendance/$ATTENDANCE_ID" \
    -H "Authorization: Bearer $MGR_TOKEN" -H "Content-Type: application/json" \
    -d "{\"attendanceId\":\"$ATTENDANCE_ID\",\"checkInTime\":null,\"checkOutTime\":null,
         \"status\":null,\"notes\":\"Manually adjusted by manager test\"}")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "PUT /api/attendance/{id} (manager edit) → 200" || fail "PUT /api/attendance/{id} → $HTTP | $(echo "$RESP" | sed '$d')"

  # 10e. Confirm attendance
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/attendance/$ATTENDANCE_ID/confirm" \
    -H "Authorization: Bearer $MGR_TOKEN")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "POST /api/attendance/{id}/confirm → 200" || fail "POST /api/attendance/{id}/confirm → $HTTP | $(echo "$RESP" | sed '$d')"
else
  echo -e "${YELLOW}SKIP${NC}: Attendance edit/confirm (no attendance records in date range)"
fi

# ─── 11. EMPLOYEE DEACTIVATE ─────────────────────────────────────────────────
section "11. Admin deactivates Employee"
if [[ -n "$STAFF_EMP_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X DELETE "$BASE_URL/api/employees/$STAFF_EMP_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  HTTP=$(echo "$RESP" | tail -n1)
  [[ "$HTTP" == "200" ]] && pass "DELETE /api/employees/{id} (deactivate) → 200" || fail "DELETE /api/employees/{id} → $HTTP | $(echo "$RESP" | sed '$d')"

  # Deactivated staff cannot login
  RESP=$(curl -s -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"emailOrPhone\":\"$STAFF_EMAIL\",\"password\":\"Staff@123\"}")
  HTTP=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"emailOrPhone\":\"$STAFF_EMAIL\",\"password\":\"Staff@123\"}")
  [[ "$HTTP" == "401" ]] && pass "Deactivated staff login → 401" || fail "Expected 401 after deactivate, got $HTTP"
fi

# ═══════════════════════════════════════════════════════════════════════════════
# V2 FEATURES
# ═══════════════════════════════════════════════════════════════════════════════

section "V2: LEAVE MANAGEMENT"
RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/leaves/types" \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Annual Leave","description":"Yearly leave allowance","defaultDaysPerYear":12,"requiresDocumentation":false}')
http_test "Admin create leave type" "$RESP" 200
LEAVE_TYPE_ID=$(echo "$RESP" | sed '$d' | grep -o '"data":"[^"]*"' | head -1 | cut -d'"' -f4)

RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/leaves/types" \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Sick Leave","description":"Medical leave","defaultDaysPerYear":10,"requiresDocumentation":true}')
http_test "Admin create sick leave type" "$RESP" 200

RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/leaves/types" -H "Authorization: Bearer $ADMIN_TOKEN")
http_test "Get leave types" "$RESP" 200

if [[ -n "$EMPLOYEE_ID" && -n "$LEAVE_TYPE_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/leaves/balance" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"employeeId\":\"$EMPLOYEE_ID\",\"leaveTypeId\":\"$LEAVE_TYPE_ID\",\"year\":$(date +%Y),\"totalDays\":12}")
  http_test "Configure leave balance" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/leaves/balance/$EMPLOYEE_ID?year=$(date +%Y)" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get leave balance" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/leaves/requests?pageNumber=1&pageSize=10" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get leave requests" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/leaves/requests/pending" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get pending leave requests" "$RESP" 200
fi

section "V2: PAYROLL"
if [[ -n "$EMPLOYEE_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/payroll/salary" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"employeeId\":\"$EMPLOYEE_ID\",\"baseSalary\":15000000,\"mealAllowance\":500000,\"transportAllowance\":300000,\"nightShiftAllowance\":200000,\"insuranceDeduction\":1000000,\"taxDeduction\":500000,\"effectiveFrom\":\"$(date +%Y)-01-01\"}")
  http_test "Set salary structure" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/payroll/salary/$EMPLOYEE_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get salary structure" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/payroll/records/$EMPLOYEE_ID?year=$(date +%Y)" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get payroll records" "$RESP" 200
fi

section "V2: PERFORMANCE"
if [[ -n "$EMPLOYEE_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/performance/goals" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"employeeId\":\"$EMPLOYEE_ID\",\"title\":\"Customer Satisfaction\",\"description\":\"Maintain >90% rating\",\"targetValue\":90,\"unit\":\"percent\",\"dueDate\":\"$(date +%Y)-12-31\"}")
  http_test "Create performance goal" "$RESP" 200
  GOAL_ID=$(echo "$RESP" | sed '$d' | grep -o '"data":"[^"]*"' | head -1 | cut -d'"' -f4)

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/performance/goals/$EMPLOYEE_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get performance goals" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/performance/reviews" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"employeeId\":\"$EMPLOYEE_ID\",\"period\":\"Q1-$(date +%Y)\",\"reviewType\":0}")
  http_test "Create performance review" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/performance/reviews/$EMPLOYEE_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get performance reviews" "$RESP" 200
fi

section "V2: TRAINING"
RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/training/courses" \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d '{"title":"VIP Service Excellence","description":"Customer service for VIP lounge","category":"Service","durationInHours":8,"passingScore":70}')
http_test "Create training course" "$RESP" 200
COURSE_ID=$(echo "$RESP" | sed '$d' | grep -o '"data":"[^"]*"' | head -1 | cut -d'"' -f4)

RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/training/courses" -H "Authorization: Bearer $ADMIN_TOKEN")
http_test "Get training courses" "$RESP" 200

if [[ -n "$EMPLOYEE_ID" && -n "$COURSE_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/training/enroll" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"employeeId\":\"$EMPLOYEE_ID\",\"courseId\":\"$COURSE_ID\"}")
  http_test "Enroll in course" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/training/enrollments/$EMPLOYEE_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get training enrollments" "$RESP" 200
fi

section "V2: ONBOARDING"
if [[ -n "$EMPLOYEE_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/onboarding" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"employeeId\":\"$EMPLOYEE_ID\",\"assignedMentorId\":null,\"tasks\":[{\"title\":\"Complete personal info\",\"description\":\"Fill all profile fields\",\"sortOrder\":1},{\"title\":\"Security training\",\"description\":\"Complete safety briefing\",\"sortOrder\":2}]}")
  http_test "Create onboarding" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/onboarding/$EMPLOYEE_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get onboarding" "$RESP" 200
fi

section "V2: ID CARDS"
if [[ -n "$EMPLOYEE_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/idcards" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"employeeId\":\"$EMPLOYEE_ID\",\"templateType\":\"Standard\"}")
  http_test "Issue ID card" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/idcards/$EMPLOYEE_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get employee ID cards" "$RESP" 200
fi

section "V2: DOCUMENTS"
if [[ -n "$EMPLOYEE_ID" ]]; then
  RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/documents" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
    -d "{\"employeeId\":\"$EMPLOYEE_ID\",\"title\":\"Employment Contract\",\"category\":0,\"filePath\":\"/documents/contract-001.pdf\",\"fileSize\":1024,\"contentType\":\"application/pdf\",\"isConfidential\":false}")
  http_test "Upload document" "$RESP" 200

  RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/documents/$EMPLOYEE_ID" \
    -H "Authorization: Bearer $ADMIN_TOKEN")
  http_test "Get employee documents" "$RESP" 200
fi

section "V2: ENHANCED DASHBOARDS"
RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/dashboard/admin" \
  -H "Authorization: Bearer $ADMIN_TOKEN")
http_test "Admin dashboard (V2 enhanced)" "$RESP" 200

RESP=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/dashboard/manager" \
  -H "Authorization: Bearer $ADMIN_TOKEN")
http_test "Manager dashboard (V2 enhanced)" "$RESP" 200

# ─── SUMMARY ─────────────────────────────────────────────────────────────────
echo ""
echo -e "${CYAN}═══════════════════════════════════════════════${NC}"
TOTAL=$((PASS + FAIL))
echo -e "Results: ${GREEN}PASS: $PASS${NC} / ${RED}FAIL: $FAIL${NC} / Total: $TOTAL"
if [[ $FAIL -eq 0 ]]; then
  echo -e "${GREEN}All tests passed!${NC}"
else
  echo -e "${RED}$FAIL test(s) failed.${NC}"
  exit 1
fi
