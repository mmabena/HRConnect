import { useState, useEffect } from "react";
import "./annual-leave-modal.css";
import {
  updateLeaveType,
  previewEntitlementImpact,
} from "../../api/leaveTypeApi";
import AnnualLeaveEditor from "./AnnualLeaveEditor";
import { X, Pencil } from "lucide-react";

const AnnualLeaveModal = ({
  isOpen,
  onClose,
  leaveTypes,
  onSuccess,
  selectedId,
  isViewMode,
}) => {
  const [selected, setSelected] = useState(null);
  const [isEditing, setIsEditing] = useState(!isViewMode);
  const [activeTab, setActiveTab] = useState("GROUP_A");

  useEffect(() => {
    if (isOpen) {
      setIsEditing(!isViewMode);
    }
  }, [isOpen, isViewMode]);

  useEffect(() => {
    if (!selectedId) return;
    const lt = leaveTypes.find((x) => x.id === Number(selectedId));
    setSelected(lt);
  }, [selectedId, leaveTypes]);
  if (!isOpen) return null;

  const visibleRules =
    selected?.rules?.filter((rule) => rule.groupKey === activeTab) || [];
  return (
    <div className="annual-modal-overlay" onClick={onClose}>
      <div className="annual-modal-card" onClick={(e) => e.stopPropagation()}>
        {isEditing && selected ? (
          //Edit Mode Header

          <div className="editannual-modal-header">
            <div className="editannual-modal-logo">
              <span className="edit-logo-singular">singular</span>
              <span className="edit-logo-express">express</span>
            </div>

            <h2 className="editannual-modal-title">Edit Leave Type</h2>
          </div>
        ) : (
          //View Mode Header
          <div className="annual-modal-header">
            <div className="annual-modal-header-content">
              <h2 className="annual-modal-title">Annual Leave</h2>

              <button
                type="button"
                className="annual-modal-close"
                onClick={onClose}
                aria-label="Close"
              >
                <X size={22} />
              </button>
            </div>
          </div>
        )}

        {/* ================================
            EDIT MODE
        ================================= */}
        {isEditing && selected ? (
          <div className="editannual-modal-content">
          <AnnualLeaveEditor
            leaveType={selected}
            onSuccess={onSuccess}
            onClose={onClose}
            isEditing={isEditing}
            setIsEditing={setIsEditing}
          />
        </div>
        ) : (
          <>
            {/* ================================
                VIEW MODE
            ================================= */}

            <div className="annual-modal-middle">
              <div className="annual-leave-fields">
                <div className="annual-leave-field">
                  <span className="annual-leave-label">CODE</span>
                  <span className="annual-leave-value">
                    {selected?.code || "-"}
                  </span>
                </div>

                <div className="annual-leave-field">
                  <span className="annual-leave-label">LEAVE TYPE</span>
                  <span className="annual-leave-value annual-leave-name">
                    {selected?.name || "-"}
                  </span>
                </div>
              </div>
              <div className="annual-description-section">
                <div className="annual-description-field">
                  <span className="annual-description-label">DESCRIPTION</span>

                  <span className="annual-description-value">
                    {selected?.description || "-"}
                  </span>
                </div>
              </div>
              <div className="annual-rules-section">
                <div className="annual-rules-tabs">
                  <button
                    className={activeTab === "GROUP_A" ? "active" : ""}
                    onClick={() => setActiveTab("GROUP_A")}
                  >
                    Unskilled-Middle
                  </button>
                  <button
                    className={activeTab === "SENIOR" ? "active" : ""}
                    onClick={() => setActiveTab("SENIOR")}
                  >
                    Senior
                  </button>
                  <button
                    className={activeTab === "EXECUTIVE" ? "active" : ""}
                    onClick={() => setActiveTab("EXECUTIVE")}
                  >
                    Executive
                  </button>
                </div>

                <div className="annual-rules-table">
                  <div className="annual-rules-header">
                    <span>Min Years</span>
                    <span>Max Years</span>
                    <span>Leave Days</span>
                  </div>

                  <div className="annual-rules-body">
                    {visibleRules.map((rule) => (
                      <div className="annual-rules-row" key={rule.id}>
                        <span>{rule.minYearsService}</span>

                        <span>{rule.maxYearsService ?? "-"}</span>

                        <span>{rule.daysAllocated}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
            <div className="annual-modal-footer">
              <div className="annual-modal-button-row">
                <button
                  type="button"
                  className="annual-modal-cancel"
                  onClick={onClose}
                >
                  Cancel
                </button>

                <button
                  type="button"
                  className="annual-modal-edit"
                  onClick={() => setIsEditing(true)}
                >
                  <Pencil size={24} />
                  <span>Edit Leave Type</span>
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default AnnualLeaveModal;
