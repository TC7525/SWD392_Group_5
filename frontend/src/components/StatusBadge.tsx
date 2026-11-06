import React from 'react';
import type { POStatus } from '../types/models';

export interface StatusBadgeProps {
  status: POStatus | number | string;
  size?: 'sm' | 'md' | 'lg';
  showEnglish?: boolean;
  className?: string;
}

interface StatusConfig {
  key: string;
  labelVi: string;
  labelEn: string;
  bg: string;
  color: string;
  borderColor: string;
  dotColor: string;
}

const STATUS_CONFIGS: Record<string, StatusConfig> = {
  pending: {
    key: 'Pending',
    labelVi: 'Chờ duyệt',
    labelEn: 'Pending',
    bg: '#FEF3C7',
    color: '#92400E',
    borderColor: '#FCD34D',
    dotColor: '#D97706',
  },
  approved: {
    key: 'Approved',
    labelVi: 'Đã duyệt',
    labelEn: 'Approved',
    bg: '#DBEAFE',
    color: '#1E40AF',
    borderColor: '#93C5FD',
    dotColor: '#2563EB',
  },
  rejected: {
    key: 'Rejected',
    labelVi: 'Từ chối',
    labelEn: 'Rejected',
    bg: '#FEE2E2',
    color: '#991B1B',
    borderColor: '#FCA5A5',
    dotColor: '#DC2626',
  },
  processing: {
    key: 'Processing',
    labelVi: 'Đang nhận hàng',
    labelEn: 'Processing',
    bg: '#EDE9FE',
    color: '#5B21B6',
    borderColor: '#C4B5FD',
    dotColor: '#7C3AED',
  },
  partiallyreceived: {
    key: 'PartiallyReceived',
    labelVi: 'Nhận một phần',
    labelEn: 'Partially Received',
    bg: '#FFEDD5',
    color: '#9A3412',
    borderColor: '#FDBA74',
    dotColor: '#EA580C',
  },
  completed: {
    key: 'Completed',
    labelVi: 'Hoàn thành',
    labelEn: 'Completed',
    bg: '#D1FAE5',
    color: '#065F46',
    borderColor: '#6EE7B7',
    dotColor: '#059669',
  },
};

const NUMERIC_STATUS_MAP: Record<number, string> = {
  0: 'pending',
  1: 'approved',
  2: 'rejected',
  3: 'processing',
  4: 'partiallyreceived',
  5: 'completed',
};

const normalizeStatus = (status: POStatus | number | string): StatusConfig => {
  if (typeof status === 'number') {
    const key = NUMERIC_STATUS_MAP[status] || 'pending';
    return STATUS_CONFIGS[key] || STATUS_CONFIGS.pending;
  }

  const normalized = String(status || '')
    .trim()
    .toLowerCase()
    .replace(/\s+/g, '');

  return (
    STATUS_CONFIGS[normalized] || {
      key: String(status),
      labelVi: String(status),
      labelEn: String(status),
      bg: '#F3F4F6',
      color: '#374151',
      borderColor: '#D1D5DB',
      dotColor: '#6B7280',
    }
  );
};

export const StatusBadge: React.FC<StatusBadgeProps> = ({
  status,
  size = 'md',
  showEnglish = false,
  className = '',
}) => {
  const config = normalizeStatus(status);

  const sizeStyles: Record<string, { padding: string; fontSize: string; dotSize: string }> = {
    sm: { padding: '2px 8px', fontSize: '11px', dotSize: '6px' },
    md: { padding: '4px 10px', fontSize: '12px', dotSize: '8px' },
    lg: { padding: '6px 14px', fontSize: '14px', dotSize: '10px' },
  };

  const currentSize = sizeStyles[size] || sizeStyles.md;

  return (
    <span
      className={`ewms-status-badge ${className}`}
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: '6px',
        padding: currentSize.padding,
        fontSize: currentSize.fontSize,
        fontWeight: 600,
        lineHeight: 1.2,
        borderRadius: '9999px',
        backgroundColor: config.bg,
        color: config.color,
        border: `1px solid ${config.borderColor}`,
        whiteSpace: 'nowrap',
        userSelect: 'none',
      }}
      title={`Trạng thái: ${config.labelVi} (${config.labelEn})`}
    >
      <span
        style={{
          width: currentSize.dotSize,
          height: currentSize.dotSize,
          borderRadius: '50%',
          backgroundColor: config.dotColor,
          display: 'inline-block',
          flexShrink: 0,
        }}
      />
      <span>{config.labelVi}</span>
      {showEnglish && (
        <span style={{ opacity: 0.7, fontWeight: 400, fontSize: '0.85em' }}>
          ({config.labelEn})
        </span>
      )}
    </span>
  );
};

export default StatusBadge;
