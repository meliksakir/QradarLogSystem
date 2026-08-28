import { useEffect, useState } from 'react'
import './App.css'

type Severity = 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL'
type Page = 'dashboard' | 'events' | 'logs' | 'userInput' | 'datasetImport'

interface ApiEvent {
  id: number
  qid: number
  eventName: string
  sourceIp: string
  destinationIp: string
  sourcePort: number
  destinationPort: number
  username: string
  severity: number
  severityLevel: Severity
  magnitude: number
  logSourceId: number
  logSourceName: string
  payload: string
  parseStatus: string
  processingTimeMs: number
  createdAt: string
}

interface EventItem {
  id: number
  qid: number
  eventName: string
  sourceIp: string
  destinationIp: string
  logSource: string
  severity: Severity
  processingTimeMs: number
}

interface EventsApiResponse {
  totalCount: number
  filters: {
    severity: string | null
    sourceIp: string | null
    eventName: string | null
    logSourceName: string | null
  }
  events: ApiEvent[]
}

interface SystemStatusResponse {
  status: string
  apiStatus: string
  databaseConnected: boolean
  databaseStatus: string
  totalEvents: number
  successfulEvents: number
  failedEvents: number
  lastEventAt: string | null
  message: string
}

interface ParseResponse {
  message: string
  eventData: {
    qid: number
    eventName: string
    sourceIp: string
    destinationIp: string
    sourcePort: number
    destinationPort: number
    username: string
    severity: number
    severityLevel: Severity
    magnitude: number
    logSourceId: number
    logSourceName: string
    payload: string
    parseStatus: string
    processingTimeMs: number
  }
}

type LogLevel = 'INFO' | 'ERROR'

interface LogItem {
  id: number
  time: string
  level: LogLevel
  message: string
}

interface LogsApiResponse {
  totalCount: number
  infoCount: number
  errorCount: number
  logs: LogItem[]
}

interface DatasetProcessedEvent {
  qid: number
  eventName: string
  sourceIp: string
  destinationIp: string
  sourcePort: number
  destinationPort: number
  username: string
  severity: number
  severityLevel: Severity
  magnitude: number
  logSourceId: number
  logSourceName: string
  payload: string
  parseStatus: string
  processingTimeMs: number
}

interface DatasetImportResult {
  fileName: string
  detectedFormat: string
  totalRecords: number
  successCount: number
  failedCount: number
  totalProcessingTimeMs: number
  successfulEvents: DatasetProcessedEvent[]
  failedEvents: string[]
}

interface DatasetUploadResponse {
  message: string
  result: DatasetImportResult
}

const EVENTS_API_URL = 'https://localhost:7271/api/Events'
const SYSTEM_API_URL = 'https://localhost:7271/api/System/status'
const LOGS_API_URL = 'https://localhost:7271/api/Logs'
const DATASET_API_URL = 'https://localhost:7271/api/Dataset/upload'

function App() {
  const [activePage, setActivePage] = useState<Page>('dashboard')

  // Dashboard için filtrelenmemiş kayıtlar
  const [dashboardEvents, setDashboardEvents] = useState<EventItem[]>([])
  const [dashboardApiEvents, setDashboardApiEvents] = useState<ApiEvent[]>([])

  // Events sayfasında backend tarafından filtrelenen kayıtlar
  const [events, setEvents] = useState<EventItem[]>([])
  const [filteredTotalCount, setFilteredTotalCount] = useState(0)

  const [loading, setLoading] = useState(true)
  const [eventsLoading, setEventsLoading] = useState(false)
  const [apiError, setApiError] = useState('')

  // System/status
  const [systemStatus, setSystemStatus] =
    useState<SystemStatusResponse | null>(null)

  // Gerçek application.txt logları
  const [logs, setLogs] = useState<LogItem[]>([])
  const [logTotalCount, setLogTotalCount] = useState(0)
  const [logInfoCount, setLogInfoCount] = useState(0)
  const [logErrorCount, setLogErrorCount] = useState(0)
  const [logsLoading, setLogsLoading] = useState(false)
  const [logsError, setLogsError] = useState('')

  // Backend filtreleri
  const [severityFilter, setSeverityFilter] = useState('ALL')
  const [sourceIpFilter, setSourceIpFilter] = useState('')
  const [eventNameFilter, setEventNameFilter] = useState('')
  const [logSourceFilter, setLogSourceFilter] = useState('')

  // Manuel event
  const [rawEventInput, setRawEventInput] = useState(
    'qid=9501|eventName=Frontend API Integration Test|sourceIp=192.168.1.250|destinationIp=10.0.0.250|sourcePort=52501|destinationPort=443|username=frontend.user|severity=6|magnitude=7|logSourceId=70|logSourceName=Frontend Test Client|payload=Event sent from React frontend to ASP.NET Core API.'
  )

  const [inputResult, setInputResult] = useState<EventItem | null>(null)
  const [inputMessage, setInputMessage] = useState('')
  const [inputError, setInputError] = useState('')
  const [processingInput, setProcessingInput] = useState(false)

  // Dataset Import
  const [selectedDatasetFile, setSelectedDatasetFile] = useState<File | null>(null)
  const [datasetResult, setDatasetResult] = useState<DatasetImportResult | null>(null)
  const [datasetMessage, setDatasetMessage] = useState('')
  const [datasetError, setDatasetError] = useState('')
  const [datasetUploading, setDatasetUploading] = useState(false)

  const mapApiEvents = (apiEvents: ApiEvent[]): EventItem[] => {
    return apiEvents.map((event) => ({
      id: event.id,
      qid: event.qid,
      eventName: event.eventName,
      sourceIp: event.sourceIp,
      destinationIp: event.destinationIp,
      logSource: event.logSourceName,
      severity: event.severityLevel,
      processingTimeMs: event.processingTimeMs,
    }))
  }

  // ----------------------------------------------------
  // DASHBOARD - tüm event kayıtlarını getir
  // ----------------------------------------------------

  const loadDashboardEvents = async () => {
    try {
      setApiError('')

      const response = await fetch(EVENTS_API_URL)

      if (!response.ok) {
        throw new Error(`API request failed: ${response.status}`)
      }

      const data: EventsApiResponse = await response.json()

      setDashboardApiEvents(data.events)
      setDashboardEvents(mapApiEvents(data.events))
    } catch (error) {
      console.error(error)

      setApiError(
        'API bağlantısı kurulamadı. Backend servisinin çalıştığını kontrol edin.'
      )
    }
  }

  // ----------------------------------------------------
  // SYSTEM STATUS
  // ----------------------------------------------------

  const loadSystemStatus = async () => {
    try {
      const response = await fetch(SYSTEM_API_URL)

      if (!response.ok) {
        throw new Error(
          `System status request failed: ${response.status}`
        )
      }

      const data: SystemStatusResponse = await response.json()

      setSystemStatus(data)
    } catch (error) {
      console.error(error)
      setSystemStatus(null)
    }
  }

  // ----------------------------------------------------
  // BACKEND FİLTRELİ EVENTS
  // ----------------------------------------------------

  const loadFilteredEvents = async () => {
    try {
      setEventsLoading(true)
      setApiError('')

      const params = new URLSearchParams()

      if (severityFilter !== 'ALL') {
        params.append('severity', severityFilter)
      }

      if (sourceIpFilter.trim()) {
        params.append('sourceIp', sourceIpFilter.trim())
      }

      if (eventNameFilter.trim()) {
        params.append('eventName', eventNameFilter.trim())
      }

      if (logSourceFilter.trim()) {
        params.append('logSourceName', logSourceFilter.trim())
      }

      const queryString = params.toString()

      const url = queryString
        ? `${EVENTS_API_URL}?${queryString}`
        : EVENTS_API_URL

      const response = await fetch(url)

      if (!response.ok) {
        throw new Error(`Filtered API request failed: ${response.status}`)
      }

      const data: EventsApiResponse = await response.json()

      setFilteredTotalCount(data.totalCount)
      setEvents(mapApiEvents(data.events))
    } catch (error) {
      console.error(error)

      setEvents([])
      setFilteredTotalCount(0)

      setApiError(
        'Filtrelenmiş event kayıtları backend üzerinden alınamadı.'
      )
    } finally {
      setEventsLoading(false)
    }
  }

  // ----------------------------------------------------
  // GERÇEK APPLICATION LOG'LARINI GETİR
  // ----------------------------------------------------

  const loadLogs = async () => {
    try {
      setLogsLoading(true)
      setLogsError('')

      const response = await fetch(LOGS_API_URL)

      if (!response.ok) {
        throw new Error(`Logs API request failed: ${response.status}`)
      }

      const data: LogsApiResponse = await response.json()

      setLogs(data.logs)
      setLogTotalCount(data.totalCount)
      setLogInfoCount(data.infoCount)
      setLogErrorCount(data.errorCount)
    } catch (error) {
      console.error(error)

      setLogs([])
      setLogTotalCount(0)
      setLogInfoCount(0)
      setLogErrorCount(0)
      setLogsError(
        'Log kayıtları backend üzerinden alınamadı. API servisinin çalıştığını kontrol edin.'
      )
    } finally {
      setLogsLoading(false)
    }
  }

  // ----------------------------------------------------
  // İLK YÜKLEME
  // ----------------------------------------------------

  const loadInitialData = async () => {
    try {
      setLoading(true)

      await Promise.all([
        loadDashboardEvents(),
        loadSystemStatus(),
        loadFilteredEvents(),
        loadLogs(),
      ])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadInitialData()
  }, [])

  // Logs sayfasına her geçildiğinde application.txt tekrar okunur
  useEffect(() => {
    if (activePage === 'logs') {
      loadLogs()
    }
  }, [activePage])

  // ----------------------------------------------------
  // FİLTRE DEĞİŞTİĞİNDE BACKEND'E YENİ İSTEK
  // ----------------------------------------------------

  useEffect(() => {
    if (loading) {
      return
    }

    const timer = window.setTimeout(() => {
      loadFilteredEvents()
    }, 400)

    return () => {
      window.clearTimeout(timer)
    }
  }, [
    severityFilter,
    sourceIpFilter,
    eventNameFilter,
    logSourceFilter,
  ])

  // ----------------------------------------------------
  // DASHBOARD HESAPLAMALARI
  // ----------------------------------------------------

  const totalEvents =
    systemStatus?.totalEvents ?? dashboardEvents.length

  const successfulEvents =
    systemStatus?.successfulEvents ??
    dashboardApiEvents.filter(
      (event) => event.parseStatus === 'SUCCESS'
    ).length

  const failedEvents =
    systemStatus?.failedEvents ??
    dashboardApiEvents.filter(
      (event) => event.parseStatus !== 'SUCCESS'
    ).length

  const lowCount = dashboardEvents.filter(
    (event) => event.severity === 'LOW'
  ).length

  const mediumCount = dashboardEvents.filter(
    (event) => event.severity === 'MEDIUM'
  ).length

  const highCount = dashboardEvents.filter(
    (event) => event.severity === 'HIGH'
  ).length

  const criticalCount = dashboardEvents.filter(
    (event) => event.severity === 'CRITICAL'
  ).length

  const averageProcessingTime =
    dashboardEvents.length > 0
      ? dashboardEvents.reduce(
          (total, event) =>
            total + event.processingTimeMs,
          0
        ) / dashboardEvents.length
      : 0

  // ----------------------------------------------------
  // MANUEL EVENT PROCESS
  // ----------------------------------------------------

  const handleProcessEvent = async () => {
    if (!rawEventInput.trim()) {
      setInputResult(null)
      setInputMessage('')
      setInputError('Raw event alanı boş bırakılamaz.')
      return
    }

    try {
      setProcessingInput(true)
      setInputMessage('')
      setInputError('')
      setInputResult(null)

      const response = await fetch(`${EVENTS_API_URL}/parse`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          rawEvent: rawEventInput,
        }),
      })

      const data = await response.json()

      if (!response.ok) {
        throw new Error(
          data.error ??
            data.message ??
            'Event backend tarafından işlenemedi.'
        )
      }

      const result = data as ParseResponse

      setInputResult({
        id: Date.now(),
        qid: result.eventData.qid,
        eventName: result.eventData.eventName,
        sourceIp: result.eventData.sourceIp,
        destinationIp: result.eventData.destinationIp,
        logSource: result.eventData.logSourceName,
        severity: result.eventData.severityLevel,
        processingTimeMs:
          result.eventData.processingTimeMs,
      })

      setInputMessage(
        'Event ASP.NET Core API üzerinden başarıyla işlendi ve SQL Server’a kaydedildi.'
      )

      await Promise.all([
        loadDashboardEvents(),
        loadSystemStatus(),
        loadFilteredEvents(),
        loadLogs(),
      ])
    } catch (error) {
      console.error(error)

      setInputResult(null)

      setInputError(
        error instanceof Error
          ? error.message
          : 'Event işlenirken bilinmeyen bir hata oluştu.'
      )
    } finally {
      setProcessingInput(false)
    }
  }

  // ----------------------------------------------------
  // DATASET UPLOAD
  // ----------------------------------------------------

  const handleDatasetUpload = async () => {
    if (!selectedDatasetFile) {
      setDatasetResult(null)
      setDatasetMessage('')
      setDatasetError('Lütfen önce bir dataset dosyası seçin.')
      return
    }

    try {
      setDatasetUploading(true)
      setDatasetMessage('')
      setDatasetError('')
      setDatasetResult(null)

      const formData = new FormData()
      formData.append('file', selectedDatasetFile)

      const response = await fetch(DATASET_API_URL, {
        method: 'POST',
        body: formData,
      })

      const data = await response.json()

      if (!response.ok) {
        throw new Error(
          data.error ??
            data.message ??
            'Dataset backend tarafından işlenemedi.'
        )
      }

      const uploadResponse = data as DatasetUploadResponse
      setDatasetResult(uploadResponse.result)
      setDatasetMessage(
        `Dataset başarıyla işlendi. Format: ${uploadResponse.result.detectedFormat}`
      )

      await Promise.all([
        loadDashboardEvents(),
        loadSystemStatus(),
        loadFilteredEvents(),
        loadLogs(),
      ])
    } catch (error) {
      console.error(error)
      setDatasetResult(null)
      setDatasetError(
        error instanceof Error
          ? error.message
          : 'Dataset yüklenirken bilinmeyen bir hata oluştu.'
      )
    } finally {
      setDatasetUploading(false)
    }
  }

  const datasetPreviewEvents: EventItem[] =
    datasetResult?.successfulEvents.slice(0, 5).map((event, index) => ({
      id: index + 1,
      qid: event.qid,
      eventName: event.eventName,
      sourceIp: event.sourceIp,
      destinationIp: event.destinationIp,
      logSource: event.logSourceName,
      severity: event.severityLevel,
      processingTimeMs: event.processingTimeMs,
    })) ?? []

  // ----------------------------------------------------
  // EVENT TABLE
  // ----------------------------------------------------

  const renderEventTable = (
    eventList: EventItem[],
    tableLoading = false
  ) => (
    <div className="table-wrapper">
      <table>
        <thead>
          <tr>
            <th>QID</th>
            <th>EVENT NAME</th>
            <th>SOURCE IP</th>
            <th>DESTINATION IP</th>
            <th>LOG SOURCE</th>
            <th>SEVERITY</th>
            <th>PROCESSING TIME</th>
          </tr>
        </thead>

        <tbody>
          {eventList.map((event) => (
            <tr key={event.id}>
              <td>{event.qid}</td>
              <td>{event.eventName}</td>
              <td>{event.sourceIp || '-'}</td>
              <td>{event.destinationIp || '-'}</td>
              <td>{event.logSource}</td>

              <td>
                <span
                  className={`severity-badge ${event.severity.toLowerCase()}`}
                >
                  {event.severity}
                </span>
              </td>

              <td>
                {event.processingTimeMs.toFixed(4)} ms
              </td>
            </tr>
          ))}

          {eventList.length === 0 && !tableLoading && (
            <tr>
              <td colSpan={7} className="empty-state">
                No events match the selected filters.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )

  return (
    <div className="app">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-icon">Q</div>

          <div>
            <h2>QRadar</h2>
            <span>Log Management</span>
          </div>
        </div>

        <nav>
          <button
            className={`nav-item ${
              activePage === 'dashboard'
                ? 'active'
                : ''
            }`}
            onClick={() =>
              setActivePage('dashboard')
            }
          >
            Dashboard
          </button>

          <button
            className={`nav-item ${
              activePage === 'events'
                ? 'active'
                : ''
            }`}
            onClick={() =>
              setActivePage('events')
            }
          >
            Events
          </button>

          <button
            className={`nav-item ${
              activePage === 'logs'
                ? 'active'
                : ''
            }`}
            onClick={() =>
              setActivePage('logs')
            }
          >
            Logs
          </button>

          <button
            className={`nav-item ${
              activePage === 'userInput'
                ? 'active'
                : ''
            }`}
            onClick={() =>
              setActivePage('userInput')
            }
          >
            User Input
          </button>

          <button
            className={`nav-item ${
              activePage === 'datasetImport'
                ? 'active'
                : ''
            }`}
            onClick={() =>
              setActivePage('datasetImport')
            }
          >
            Dataset Import
          </button>
        </nav>

        <div className="system-status">
          <span className="status-dot"></span>

          {systemStatus?.databaseConnected
            ? 'System Active'
            : 'System Status Unknown'}
        </div>
      </aside>

      <main className="main-content">

        {/* ==================================================
            DASHBOARD
        ================================================== */}

        {activePage === 'dashboard' && (
          <>
            <header className="page-header">
              <div>
                <p className="eyebrow">
                  SECURITY MONITORING
                </p>

                <h1>Dashboard</h1>

                <p className="subtitle">
                  QRadar event processing and system overview
                </p>
              </div>

              <div className="live-status">
                <span className="status-dot"></span>

                {loading
                  ? 'Loading...'
                  : systemStatus?.apiStatus ??
                    'API Connected'}
              </div>
            </header>

            {apiError && (
              <div className="input-message">
                {apiError}
              </div>
            )}

            <section className="stats-grid">
              <div className="stat-card">
                <span className="stat-label">
                  TOTAL EVENTS
                </span>

                <strong>{totalEvents}</strong>

                <small>Processed events</small>
              </div>

              <div className="stat-card">
                <span className="stat-label">
                  SUCCESS
                </span>

                <strong>
                  {successfulEvents}
                </strong>

                <small>
                  Successfully processed
                </small>
              </div>

              <div className="stat-card">
                <span className="stat-label">
                  FAILED
                </span>

                <strong>{failedEvents}</strong>

                <small>Failed events</small>
              </div>

              <div className="stat-card">
                <span className="stat-label">
                  AVG. PROCESSING
                </span>

                <strong>
                  {averageProcessingTime.toFixed(2)} ms
                </strong>

                <small>
                  Average processing time
                </small>
              </div>
            </section>

            {/* SYSTEM STATUS */}

            <section className="severity-section">
              <div className="section-heading">
                <div>
                  <h2>System Status</h2>

                  <p>
                    ASP.NET Core API and SQL Server health information
                  </p>
                </div>

                <button
                  className="view-button"
                  onClick={loadSystemStatus}
                >
                  Refresh Status
                </button>
              </div>

              <div className="severity-grid">
                <div className="severity-card low">
                  <span>API STATUS</span>

                  <strong>
                    {systemStatus?.apiStatus ??
                      'UNKNOWN'}
                  </strong>
                </div>

                <div className="severity-card low">
                  <span>DATABASE</span>

                  <strong>
                    {systemStatus?.databaseConnected
                      ? 'CONNECTED'
                      : 'UNKNOWN'}
                  </strong>
                </div>

                <div className="severity-card medium">
                  <span>TOTAL RECORDS</span>

                  <strong>
                    {systemStatus?.totalEvents ??
                      totalEvents}
                  </strong>
                </div>

                <div className="severity-card high">
                  <span>LAST EVENT</span>

                  <strong
                    style={{
                      fontSize: '13px',
                      lineHeight: '1.5',
                    }}
                  >
                    {systemStatus?.lastEventAt
                      ? new Date(
                          systemStatus.lastEventAt
                        ).toLocaleString('tr-TR')
                      : '-'}
                  </strong>
                </div>
              </div>
            </section>

            {/* SEVERITY */}

            <section className="severity-section">
              <div className="section-heading">
                <div>
                  <h2>Severity Overview</h2>

                  <p>
                    Distribution of processed security events
                  </p>
                </div>
              </div>

              <div className="severity-grid">
                <div className="severity-card low">
                  <span>LOW</span>
                  <strong>{lowCount}</strong>
                </div>

                <div className="severity-card medium">
                  <span>MEDIUM</span>
                  <strong>{mediumCount}</strong>
                </div>

                <div className="severity-card high">
                  <span>HIGH</span>
                  <strong>{highCount}</strong>
                </div>

                <div className="severity-card critical">
                  <span>CRITICAL</span>
                  <strong>{criticalCount}</strong>
                </div>
              </div>
            </section>

            {/* LATEST EVENTS */}

            <section className="events-section">
              <div className="section-heading">
                <div>
                  <h2>Latest Events</h2>

                  <p>
                    Events retrieved from SQL Server through ASP.NET Core API
                  </p>
                </div>

                <button
                  className="view-button"
                  onClick={() =>
                    setActivePage('events')
                  }
                >
                  View All Events
                </button>
              </div>

              {loading ? (
                <div className="empty-state">
                  Loading events from API...
                </div>
              ) : (
                renderEventTable(
                  dashboardEvents.slice(0, 5)
                )
              )}
            </section>
          </>
        )}

        {/* ==================================================
            EVENTS
        ================================================== */}

        {activePage === 'events' && (
          <>
            <header className="page-header">
              <div>
                <p className="eyebrow">
                  EVENT MANAGEMENT
                </p>

                <h1>Events</h1>

                <p className="subtitle">
                  Search and filter events retrieved from SQL Server
                </p>
              </div>

              <div className="live-status">
                <span className="status-dot"></span>
                {filteredTotalCount} Events
              </div>
            </header>

            <section className="filter-section">
              <div className="filter-grid">

                <div className="filter-field">
                  <label>Severity</label>

                  <select
                    value={severityFilter}
                    onChange={(e) =>
                      setSeverityFilter(
                        e.target.value
                      )
                    }
                  >
                    <option value="ALL">
                      All Severities
                    </option>

                    <option value="LOW">
                      LOW
                    </option>

                    <option value="MEDIUM">
                      MEDIUM
                    </option>

                    <option value="HIGH">
                      HIGH
                    </option>

                    <option value="CRITICAL">
                      CRITICAL
                    </option>
                  </select>
                </div>

                <div className="filter-field">
                  <label>Source IP</label>

                  <input
                    type="text"
                    placeholder="192.168..."
                    value={sourceIpFilter}
                    onChange={(e) =>
                      setSourceIpFilter(
                        e.target.value
                      )
                    }
                  />
                </div>

                <div className="filter-field">
                  <label>Event Name</label>

                  <input
                    type="text"
                    placeholder="Login, Firewall..."
                    value={eventNameFilter}
                    onChange={(e) =>
                      setEventNameFilter(
                        e.target.value
                      )
                    }
                  />
                </div>

                <div className="filter-field">
                  <label>Log Source</label>

                  <input
                    type="text"
                    placeholder="Windows Server, Linux Server..."
                    value={logSourceFilter}
                    onChange={(e) =>
                      setLogSourceFilter(
                        e.target.value
                      )
                    }
                  />
                </div>
              </div>

              <div className="filter-footer">
                <span>
                  Backend API returned{' '}
                  {filteredTotalCount} event(s)
                </span>

                <button
                  className="clear-button"
                  onClick={() => {
                    setSeverityFilter('ALL')
                    setSourceIpFilter('')
                    setEventNameFilter('')
                    setLogSourceFilter('')
                  }}
                >
                  Clear Filters
                </button>
              </div>
            </section>

            <section className="events-section">
              <div className="section-heading">
                <div>
                  <h2>Event Records</h2>

                  <p>
                    Live filtered records retrieved through the backend API
                  </p>
                </div>

                <button
                  className="view-button"
                  onClick={loadFilteredEvents}
                >
                  Refresh
                </button>
              </div>

              {eventsLoading ? (
                <div className="empty-state">
                  Querying SQL Server through API...
                </div>
              ) : (
                renderEventTable(
                  events,
                  eventsLoading
                )
              )}
            </section>
          </>
        )}

        {/* ==================================================
            LOGS
        ================================================== */}

        {activePage === 'logs' && (
          <>
            <header className="page-header">
              <div>
                <p className="eyebrow">
                  SYSTEM LOGGING
                </p>

                <h1>Logs</h1>

                <p className="subtitle">
                  Application processing and error logs
                </p>
              </div>

              <div className="live-status">
                <span className="status-dot"></span>
                {logsLoading
                  ? 'Loading...'
                  : `${logTotalCount} Log Records`}
              </div>
            </header>

            {logsError && (
              <div className="input-message">
                {logsError}
              </div>
            )}

            <section className="log-summary-grid">
              <div className="log-summary-card">
                <span>TOTAL LOGS</span>
                <strong>{logTotalCount}</strong>
              </div>

              <div className="log-summary-card info">
                <span>INFO</span>
                <strong>{logInfoCount}</strong>
              </div>

              <div className="log-summary-card error">
                <span>ERROR</span>
                <strong>{logErrorCount}</strong>
              </div>
            </section>

            <section className="logs-section">
              <div className="section-heading">
                <div>
                  <h2>Application Logs</h2>

                  <p>
                    Real-time processing results read from application.txt through ASP.NET Core API
                  </p>
                </div>

                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '12px',
                  }}
                >
                  <span className="log-file-name">
                    application.txt
                  </span>

                  <button
                    className="view-button"
                    onClick={loadLogs}
                    disabled={logsLoading}
                  >
                    {logsLoading ? 'Loading...' : 'Refresh Logs'}
                  </button>
                </div>
              </div>

              {logsLoading && logs.length === 0 ? (
                <div className="empty-state">
                  Loading application logs from API...
                </div>
              ) : logs.length === 0 ? (
                <div className="empty-state">
                  No application logs were found.
                </div>
              ) : (
                <div className="log-list">
                  {[...logs].reverse().map((log) => (
                    <div
                      className={`log-row ${log.level.toLowerCase()}`}
                      key={log.id}
                    >
                      <span className="log-time">
                        [{log.time}]
                      </span>

                      <span
                        className={`log-level ${log.level.toLowerCase()}`}
                      >
                        {log.level}
                      </span>

                      <span className="log-message">
                        {log.message}
                      </span>
                    </div>
                  ))}
                </div>
              )}
            </section>
          </>
        )}

        {/* ==================================================
            USER INPUT
        ================================================== */}

        {activePage === 'userInput' && (
          <>
            <header className="page-header">
              <div>
                <p className="eyebrow">
                  MANUAL EVENT INPUT
                </p>

                <h1>User Input</h1>

                <p className="subtitle">
                  Send a raw QRadar event to the ASP.NET Core processing API
                </p>
              </div>

              <div className="live-status">
                <span className="status-dot"></span>
                API Mode
              </div>
            </header>

            <section className="input-section">
              <div className="section-heading">
                <div>
                  <h2>Raw Event Input</h2>

                  <p>
                    Enter a QRadar event in key=value format
                  </p>
                </div>
              </div>

              <textarea
                className="raw-event-input"
                value={rawEventInput}
                onChange={(e) =>
                  setRawEventInput(
                    e.target.value
                  )
                }
              />

              <div className="input-actions">
                <button
                  className="process-button"
                  disabled={processingInput}
                  onClick={handleProcessEvent}
                >
                  {processingInput
                    ? 'Processing...'
                    : 'Process Event'}
                </button>

                <button
                  className="clear-button"
                  disabled={processingInput}
                  onClick={() => {
                    setRawEventInput('')
                    setInputResult(null)
                    setInputMessage('')
                    setInputError('')
                  }}
                >
                  Clear
                </button>
              </div>

              {inputMessage && (
                <div className="input-message">
                  {inputMessage}
                </div>
              )}

              {inputError && (
                <div className="input-message">
                  {inputError}
                </div>
              )}
            </section>

            {inputResult && (
              <section className="result-section">
                <div className="section-heading">
                  <div>
                    <h2>Processed Result</h2>

                    <p>
                      Parse and normalization result returned by ASP.NET Core API
                    </p>
                  </div>
                </div>

                <div className="result-grid">
                  <div>
                    <span>QID</span>
                    <strong>
                      {inputResult.qid}
                    </strong>
                  </div>

                  <div>
                    <span>Event Name</span>
                    <strong>
                      {inputResult.eventName}
                    </strong>
                  </div>

                  <div>
                    <span>Source IP</span>
                    <strong>
                      {inputResult.sourceIp}
                    </strong>
                  </div>

                  <div>
                    <span>Destination IP</span>
                    <strong>
                      {inputResult.destinationIp}
                    </strong>
                  </div>

                  <div>
                    <span>Log Source</span>
                    <strong>
                      {inputResult.logSource}
                    </strong>
                  </div>

                  <div>
                    <span>Severity</span>
                    <strong>
                      {inputResult.severity}
                    </strong>
                  </div>

                  <div>
                    <span>Processing Time</span>

                    <strong>
                      {inputResult.processingTimeMs.toFixed(
                        4
                      )}{' '}
                      ms
                    </strong>
                  </div>
                </div>
              </section>
            )}
          </>
        )}

        {/* ==================================================
            DATASET IMPORT
        ================================================== */}

        {activePage === 'datasetImport' && (
          <>
            <header className="page-header">
              <div>
                <p className="eyebrow">
                  DATASET PROCESSING
                </p>

                <h1>Dataset Import</h1>

                <p className="subtitle">
                  Upload LEEF, CSV, JSON or JSONL datasets and process them through the ASP.NET Core API
                </p>
              </div>

              <div className="live-status">
                <span className="status-dot"></span>
                Multi-format Import
              </div>
            </header>

            <section className="input-section dataset-upload-section">
              <div className="section-heading">
                <div>
                  <h2>Upload Dataset</h2>
                  <p>
                    Supported formats: LEEF, CSV, JSON, JSONL and key=value text datasets
                  </p>
                </div>
              </div>

              <label
                className="dataset-file-picker"
                onDragOver={(e) => {
                  e.preventDefault()
                }}
                onDrop={(e) => {
                  e.preventDefault()

                  const file = e.dataTransfer.files?.[0] ?? null

                  if (file) {
                    setSelectedDatasetFile(file)
                    setDatasetResult(null)
                    setDatasetMessage('')
                    setDatasetError('')
                  }
                }}
              >
                <input
                  type="file"
                  accept=".txt,.log,.leef,.csv,.json,.jsonl,.xml"
                  onChange={(e) => {
                    const file = e.target.files?.[0] ?? null
                    setSelectedDatasetFile(file)
                    setDatasetResult(null)
                    setDatasetMessage('')
                    setDatasetError('')
                  }}
                />
                <span className="dataset-file-picker-title">
                  {selectedDatasetFile
                    ? 'Dataset Selected'
                    : 'Choose or Drop Dataset File'}
                </span>
                <span className="dataset-file-picker-subtitle">
                  {selectedDatasetFile
                    ? selectedDatasetFile.name
                    : 'Click here or drag & drop a dataset file'}
                </span>
              </label>

              {selectedDatasetFile && (
                <div className="dataset-file-details">
                  <div>
                    <span>FILE NAME</span>
                    <strong>{selectedDatasetFile.name}</strong>
                  </div>
                  <div>
                    <span>FILE TYPE</span>
                    <strong>{selectedDatasetFile.type || 'Detected by backend'}</strong>
                  </div>
                  <div>
                    <span>FILE SIZE</span>
                    <strong>{(selectedDatasetFile.size / 1024).toFixed(2)} KB</strong>
                  </div>
                </div>
              )}

              <div className="input-actions">
                <button
                  className="process-button"
                  disabled={datasetUploading || !selectedDatasetFile}
                  onClick={handleDatasetUpload}
                >
                  {datasetUploading
                    ? 'Uploading & Processing...'
                    : 'Upload & Process Dataset'}
                </button>

                <button
                  className="clear-button"
                  disabled={datasetUploading}
                  onClick={() => {
                    setSelectedDatasetFile(null)
                    setDatasetResult(null)
                    setDatasetMessage('')
                    setDatasetError('')
                  }}
                >
                  Clear
                </button>
              </div>

              {datasetMessage && (
                <div className="input-message">
                  {datasetMessage}
                </div>
              )}

              {datasetError && (
                <div className="dataset-error-message">
                  {datasetError}
                </div>
              )}
            </section>

            {datasetResult && (
              <>
                <section className="dataset-summary-grid">
                  <div className="dataset-summary-card format">
                    <span>DETECTED FORMAT</span>
                    <strong>{datasetResult.detectedFormat}</strong>
                  </div>

                  <div className="dataset-summary-card">
                    <span>TOTAL RECORDS</span>
                    <strong>{datasetResult.totalRecords}</strong>
                  </div>

                  <div className="dataset-summary-card success">
                    <span>SUCCESS</span>
                    <strong>{datasetResult.successCount}</strong>
                  </div>

                  <div className="dataset-summary-card error">
                    <span>FAILED</span>
                    <strong>{datasetResult.failedCount}</strong>
                  </div>
                </section>

                <section className="result-section">
                  <div className="section-heading">
                    <div>
                      <h2>Import Result</h2>
                      <p>
                        Dataset processing summary returned by ASP.NET Core API
                      </p>
                    </div>
                  </div>

                  <div className="result-grid">
                    <div>
                      <span>File Name</span>
                      <strong>{datasetResult.fileName}</strong>
                    </div>
                    <div>
                      <span>Detected Format</span>
                      <strong>{datasetResult.detectedFormat}</strong>
                    </div>
                    <div>
                      <span>Total Records</span>
                      <strong>{datasetResult.totalRecords}</strong>
                    </div>
                    <div>
                      <span>Successful</span>
                      <strong>{datasetResult.successCount}</strong>
                    </div>
                    <div>
                      <span>Failed</span>
                      <strong>{datasetResult.failedCount}</strong>
                    </div>
                    <div>
                      <span>Total Processing Time</span>
                      <strong>{datasetResult.totalProcessingTimeMs.toFixed(4)} ms</strong>
                    </div>
                  </div>
                </section>

                <section className="events-section">
                  <div className="section-heading">
                    <div>
                      <h2>Processed Event Preview</h2>
                      <p>
                        First {Math.min(datasetResult.successCount, 5)} successfully processed event(s)
                      </p>
                    </div>

                    <button
                      className="view-button"
                      onClick={() => setActivePage('events')}
                    >
                      View All Events
                    </button>
                  </div>

                  {datasetPreviewEvents.length > 0 ? (
                    renderEventTable(datasetPreviewEvents)
                  ) : (
                    <div className="empty-state">
                      No successfully processed events are available for preview.
                    </div>
                  )}
                </section>

                {datasetResult.failedEvents.length > 0 && (
                  <section className="logs-section">
                    <div className="section-heading">
                      <div>
                        <h2>Failed Records</h2>
                        <p>
                          Records that could not be processed from the uploaded dataset
                        </p>
                      </div>
                    </div>

                    <div className="dataset-failed-list">
                      {datasetResult.failedEvents.map((failedEvent, index) => (
                        <div key={`${index}-${failedEvent}`}>
                          {failedEvent}
                        </div>
                      ))}
                    </div>
                  </section>
                )}
              </>
            )}
          </>
        )}
      </main>
    </div>
  )
}

export default App