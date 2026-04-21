import { useEffect, useState } from "react";
import { getLeaveTypes } from "../../api/leaveTypeApi";
import "./leave-tables.css";
import "../../Components/MenuBar/MenuBar.css";
import NavBar from "../NavBar";
import AddLeaveTypeModal from "./AddLeaveTypeModal";
import EditLeaveTypeModal from "./EditLeaveTypeModal";
import { toggleLeaveTypeStatus } from "../../api/leaveTypeApi";
import ConfirmStatusModal from "./ConfirmStatusModal";
import { Dot } from 'lucide-react';


const LeaveTables = () => {
  const [active, setActive] = useState([]);
  const [inactive, setInactive] = useState([]);
  const [showModal, setShowModal] = useState(false);
  const [showEdit, setShowEdit] = useState(false);
  const [selectedLeaveId, setSelectedLeaveId] = useState("");
  const [isViewMode, setIsViewMode] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState(null);

  const splitData = (data) => {
    setActive(data.filter(x => x.isActive));
    setInactive(data.filter(x => !x.isActive));
  };
const fetchData = async () => {
  const data = await getLeaveTypes();
  console.log("LEAVE TYPES DATA:", data);
  splitData(data);
};

useEffect(() => {
  fetchData();
}, []);

const handleToggleClick = (item) => {
  setSelectedItem(item);
  setConfirmOpen(true);
};

const confirmToggle = async () => {
  try {
    const result = await toggleLeaveTypeStatus(selectedItem.id);

    if (result.isActive) {
      setActive(prev => [...prev, selectedItem]);
      setInactive(prev => prev.filter(x => x.id !== selectedItem.id));
    } else {
      setInactive(prev => [...prev, selectedItem]);
      setActive(prev => prev.filter(x => x.id !== selectedItem.id));
    }

    setConfirmOpen(false);
    setSelectedItem(null);

  } catch (err) {
    console.error("Status change failed:", err);
  }
};

  // Handles entitlement display logic
  const getEntitlement = (rules) => {
  if (!rules || rules.length === 0) return "-";

  // Get unique days values
  const activeRules = rules.filter(r => r.isActive !== false);
  const uniqueDays = [...new Set(activeRules.map(r => r.daysAllocated))];

  // If all rules have SAME value → show it
  if (uniqueDays.length === 1) {
    return `${uniqueDays[0]} Days`;
  }

  // Different values (Annual Leave) → show dash
  return "-";
};

  return (
     <div className="menu-background custom-scrollbar">

    <div className="wrap-container">
      <div className="heading-container">Company Management</div>
    </div>

    <div className="navbar-with-button">
      <NavBar />
    </div>
  <div className="lt-page-container">


    <div className="lt-top-section">
      <button className="lt-add-btn" onClick={() => setShowModal(true)}>
          + Add Leave Type
      </button>
    </div>

     {/* ================= ACTIVE TABLE ================= */}
      <div className="lt-box">
        <div className="lt-header">
          <span>Leave History</span>
        </div>

        <table className="lt-table">
          <thead>
            <tr>
              <th>Code</th>
              <th>Name</th>
              <th>Description</th>
              <th>Leave Entitlement</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>

          <tbody>
            {active.map(item => (
              <tr key={item.id}>
                <td className="lt-code">{item.code}</td>
                <td className="lt-name">{item.name}</td>
                <td className="lt-description">{item.description || "-"}</td>
                <td className="lt-entitlement">{getEntitlement(item.rules)}</td>

                <td>
                  <span
                    className="lt-status active"
                    onClick={() => handleToggleClick(item)}
                  >
                    <Dot className="status-dot" /> Active
                  </span>
                </td>

                <td className="lt-actions">
  <span
    onClick={() => {
      setSelectedLeaveId(item.id);
      setIsViewMode(true);
      setShowEdit(true);
    }}
  >
    View
  </span>

  <span className="divider">|</span>

  <span className="edit-table-actions"
    onClick={() => {
      setSelectedLeaveId(item.id);
      setIsViewMode(false);
      setShowEdit(true);
    }}
  >
    Edit
  </span>
</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

   {/* ================= INACTIVE TABLE ================= */}
      <div className="lt-box">
        <div className="lt-header">
          <span>Leave Definitions</span>
        </div>

        <table className="lt-table">
          <thead>
            <tr>
              <th>Code</th>
              <th>Name</th>
              <th>Description</th>
              <th>Leave Entitlement</th>
              <th>Status</th>
              <th>Comment</th>
            </tr>
          </thead>

          <tbody>
            {inactive.map(item => (
              <tr key={item.id}>
                <td className="lt-code inactive-code">{item.code}</td>
                <td className="lt-name">{item.name}</td>
                <td className="lt-description">{item.description || "-"}</td>
                <td className="lt-entitlement">{getEntitlement(item.rules)}</td>
                <td>
                  <span
                    className="lt-status inactive"
                    onClick={() => handleToggleClick(item)}
                  >
                    <Dot className="status-dot" /> Inactive
                  </span>
                </td>
                <td className="lt-comment">-</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
  </div> 
  <AddLeaveTypeModal
  isOpen={showModal}
  onClose={() => setShowModal(false)}
  onSuccess={fetchData}
  />
  
  <EditLeaveTypeModal
  isOpen={showEdit}
  onClose={() => setShowEdit(false)}
  leaveTypes={[...active, ...inactive]}
  selectedId={selectedLeaveId}
  onSuccess={fetchData}
  isViewMode={isViewMode}
/>
<ConfirmStatusModal
  isOpen={confirmOpen}
  onClose={() => setConfirmOpen(false)}
  onConfirm={confirmToggle}
  isActive={selectedItem?.isActive}
/>
    </div>
    
  );
};
export default LeaveTables;