import React, { useState, useEffect, useCallback } from "react";
import api from "../../../api/api.js";
import "../../../styles/global.css";
import TaxTableUpload from "./TaxTableUpload";
import NavBar from "../../NavBar.jsx";

function TaxTableManagement({ currentUser }) {
  const [activeTable, setActiveTable] = useState(null);
  const [futureTable, setFutureTable] = useState(null);
  const [previousTables, setPreviousTables] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showUploadPopup, setShowUploadPopup] = useState(false);
  const [currentTime, setCurrentTime] = useState("");
  const [currentDate, setCurrentDate] = useState("");

  const fetchTaxTables = useCallback(async () => {
    try {
      const response = await api.get("/taxtableupload");
      const tables = Array.isArray(response.data)
        ? response.data
        : response.data?.data || [];

      if (!tables.length) {
        setActiveTable(null);
        setFutureTable(null);
        setPreviousTables([]);
        return;
      }

      // Normalize today (remove time)
      // Get today's date in YYYY-MM-DD format
      const todayString = new Date().toISOString().split("T")[0];

      const normalized = tables
        .map((t) => ({
          ...t,
          effectiveDateString: t.effectiveFrom.split("T")[0], // strip time
        }))
        .sort((a, b) =>
          b.effectiveDateString.localeCompare(a.effectiveDateString),
        );

      /* -------- ACTIVE TABLE --------
       */
      const activeCandidates = normalized
        .filter((t) => t.effectiveDateString <= todayString)
        .sort((a, b) =>
          b.effectiveDateString.localeCompare(a.effectiveDateString),
        );

      const active = activeCandidates[0] || null;

      const futureCandidates = normalized
        .filter((t) => t.effectiveDateString > todayString)
        .sort((a, b) =>
          a.effectiveDateString.localeCompare(b.effectiveDateString),
        );

      const future = futureCandidates[0] || null;

      /* -------- PREVIOUS TABLES --------
       */
      const previous = active
        ? normalized.filter(
            (t) => t.effectiveDateString < active.effectiveDateString,
          )
        : [];

      setActiveTable(active);
      setFutureTable(future);
      setPreviousTables(previous);
    } catch (err) {
      console.error("API ERROR:", err.response?.data || err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchTaxTables();
  }, [fetchTaxTables]);

  useEffect(() => {
    const interval = setInterval(fetchTaxTables, 60000);
    return () => clearInterval(interval);
  }, [fetchTaxTables]);

  useEffect(() => {
    const updateClock = () => {
      const now = new Date();
      setCurrentTime(
        now.toLocaleTimeString("en-US", {
          hour: "numeric",
          minute: "2-digit",
          hour12: true,
        }),
      );
      setCurrentDate(
        now.toLocaleDateString("en-US", {
          year: "numeric",
          month: "short",
          day: "numeric",
        }),
      );
    };

    updateClock();
    const interval = setInterval(updateClock, 60000);
    return () => clearInterval(interval);
  }, []);

  if (loading) return <div>Loading...</div>;

  const formatDate = (date) =>
    new Date(date).toLocaleDateString("en-GB", {
      day: "2-digit",
      month: "long",
      year: "numeric",
    });

  const taxRange = (year) => `March ${year} - February ${Number(year) + 1}`;

  return (
    <div className="menu-background custom-scrollbar">
      <div className="wrap-container">
        <div className="heading-container">Comapany Management</div>
      </div>
      <div className="navbar-with-button">
        <NavBar />
      </div>

      {/* ================= ACTIVE SECTION ================= */}
      <div className="card-container">
        <div className="card-header">
          <h3>Active Tax Table</h3>
          <button
            className="upload-btn"
            onClick={() => setShowUploadPopup(true)}
          >
            Upload Tax Table
          </button>
        </div>

        {(futureTable || activeTable) && (
          <>
            <div className="status-row">
              <div className="status-info">
                <span className="label">Status:</span>

                {futureTable ? (
                  <span className="status-badge uploaded">Uploaded</span>
                ) : (
                  <span className="status-badge active">Active</span>
                )}

                <span className="label effective-label">Effective Date:</span>

                <span className="date-value">
                  {futureTable
                    ? formatDate(futureTable.effectiveFrom)
                    : formatDate(activeTable?.effectiveFrom)}
                </span>
              </div>
            </div>

            <table className="custom-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Year</th>
                  <th>Tax Range</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {futureTable && (
                  <tr>
                    <td>{futureTable.fileName}</td>
                    <td>{futureTable.taxYear}</td>
                    <td>{taxRange(futureTable.taxYear)}</td>
                    <td>
                      <span className="status">-</span>
                    </td>
                  </tr>
                )}

                {activeTable && (
                  <tr>
                    <td>{activeTable.fileName}</td>
                    <td>{activeTable.taxYear}</td>
                    <td>{taxRange(activeTable.taxYear)}</td>
                    <td>
                      <span className="status-active">Active</span>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </>
        )}
      </div>

      {/* ================= PREVIOUS SECTION ================= */}
      <div className="card-container">
        <div className="card-header">
          <h3>Previous Tax Tables</h3>
        </div>

        <table className="custom-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Year</th>
              <th>Tax Range</th>
            </tr>
          </thead>
          <tbody>
            {previousTables.length > 0 ? (
              previousTables.map((table) => (
                <tr key={table.id} className="previous-row">
                  <td>{table.fileName}</td>
                  <td>{table.taxYear}</td>
                  <td>{taxRange(table.taxYear)}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan="3">No previous tax tables</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* ================= UPLOAD POPUP ================= */}
      {showUploadPopup && (
        <div className="modal-overlay">
          <TaxTableUpload
            onClose={() => setShowUploadPopup(false)}
            onUploadSuccess={() => {
              fetchTaxTables();
              setShowUploadPopup(false);
            }}
          />
        </div>
      )}
    </div>
  );
}

export default TaxTableManagement;
