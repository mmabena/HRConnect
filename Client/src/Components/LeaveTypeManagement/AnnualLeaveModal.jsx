import { useState, useEffect } from "react";
import "./annual-leave-modal.css";
import {
  updateLeaveType,
  previewEntitlementImpact,
} from "../../api/leaveTypeApi";
import AnnualLeaveEditor from "./AnnualLeaveEditor";
import { X } from "lucide-react";

const AnnualLeaveModal = ({
  isOpen,
  onClose,
  leaveTypes,
  onSuccess,
  selectedId,
  isViewMode,
}) => {
  const [selected, setSelected] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  useEffect(() => {
    setIsEditing(!isViewMode);
  }, [isViewMode]);
  useEffect(() => {
    if (!selectedId) return;
    const lt = leaveTypes.find((x) => x.id === Number(selectedId));
    setSelected(lt);
  }, [selectedId, leaveTypes]);
  if (!isOpen) return null;
  return (
    <div className="annual-modal-overlay" onClick={onClose}>
      <div className="annual-modal-card" onClick={(e) => e.stopPropagation()}>
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
        </div>
      </div>
    </div>
  );
};

export default AnnualLeaveModal;
