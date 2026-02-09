# Logging & Monitoring Guide

This guide explains how to effectively use the logging infrastructure (Serilog + Seq) in the Resource Management Tool.

## 1. Accessing the Logs

The application is configured to send logs to a **Seq** server.

- **Production URL**: `http://<your-production-server-ip>:5341` (Replace with actual IP/Domain)
- **Local Development**: `http://localhost:5341`

## 2. Searching for Failures

Seq allows you to search logs using a SQL-like syntax or simple text search.

### Basic Searching
- **Text Search**: Just type a keyword, e.g., `error`, `exception`, or a specific ID like `Project-123`.
- **Filter by Level**: Click on the "Level" column header or type `Level = 'Error'` to see only errors.

### Advanced Queries (SQL-like)
Seq supports complex queries. Here are some useful patterns:

**Find all errors in the last 24 hours:**
```sql
@Level = 'Error' and @Timestamp > Now() - 1d
```

**Find logs for a specific Request (Correlation ID):**
All logs for a single HTTP request share a `CorrelationId`.
```sql
CorrelationId = 'gw-5b3a1...'
```

**Find long-running requests (> 1 second):**
```sql
DurationMs > 1000
```

**Find specific exceptions:**
```sql
@Exception like '%NullReferenceException%'
```

## 3. Creating Dashboards

Dashboards in Seq provide a high-level view of your application's health.

### Steps to Create a Dashboard:
1. **Navigate to "Dashboards"**: Click the "Dashboards" tab on the top navigation bar.
2. **Add Dashboard**: Click "Add Dashboard" and give it a name (e.g., "Application Health").
3. **Add Charts**:
   - **Signal/Query**: First, go to the "Events" view and write a query (e.g., `Level = 'Error'`).
   - **Pin to Dashboard**: Click the pin icon (📌) next to the search bar.
   - **Configure**: specific the chart type (Count, Time Series, etc.).

### Recommended Charts:
1. **Error Rate (Timeseries)**:
   - Query: `@Level = 'Error'`
   - Visualization: file:///c:/Users/gtsolakidis/.gemini/antigravity/scratch/ResourceManagementTool/docs/assets/error_rate_chart.png (Mockup)
2. **Request Duration (Heatmap/Average)**:
   - Query: `select avg(DurationMs) from stream group by time(1m)`
3. **Top Exceptions (Table)**:
   - Query: `select count(*) as Count from stream where @Exception is not null group by @Exception`

## 4. Improving Logging Context

We have enriched the logs with additional context. You can now filter by:

- **MachineName**: Verify which server processed the request.
- **ThreadId**: useful for debugging concurrency issues.
- **ProcessId**: Identify checking process restarts.
- **EnvironmentUserName**: The user running the application process.

## 5. Middleware Logging

We have `AuditLoggingMiddleware` that captures:
- **Request Body**: (Truncated to 32KB)
- **Response Body**: (Truncated to 32KB)
- **User Identity**: `UserId`, `Username`

Use these fields to debug "what specific data caused this error?".

### Example: Find failed POST requests
```sql
HttpMethod = 'POST' and StatusCode >= 400
```

## 6. Setting up Alerts

Seq allows you to send notifications (Email, Slack, Teams) when specific criteria are met. This is done via **Seq Apps**.

### Prerequisite
Ensure the relevant Seq App (e.g., "Email", "Slack") is installed on your Seq server instance (Settings > Apps).

### Creating an Alert

1.  **Define the Signal**: 
    - Create a query for the condition you want to alert on.
    - Example: `Level = 'Error'`
2.  **Add Instance**:
    - Go to "Settings" > "Apps".
    - Click "Start new instance" next to the notification app (e.g., Email).
3.  **Configure**:
    - **Title**: "Production Errors"
    - **Signal**: Select the signal you created in step 1.
    - **Threshold**: (Optional) Only alert if more than X events occur in Y minutes.
    - **Details**: Enter recipient email, channel URL, etc.

### Recommended Alerts

#### 1. High Criticality Errors
Trigger an immediate alert for any `Fatal` or critical system error.
- **Query**: `@Level = 'Fatal'`
- **Threshold**: 0 (Immediate)

#### 2. Sustained Error Rate
Alert if the application is throwing many errors, indicating a systemic issue.
- **Query**: `@Level = 'Error'`
- **Threshold**: > 10 events in 1 minute

#### 3. Slow Performance (SLO Breach)
Alert if many requests are taking longer than acceptable limits (e.g., 2 seconds).
- **Query**: `DurationMs > 2000`
- **Threshold**: > 5 events in 5 minutes
