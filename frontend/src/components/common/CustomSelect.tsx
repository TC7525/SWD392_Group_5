import React, { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';

export interface SelectOption {
  value: string | number;
  label: string;
  subLabel?: string;
}

interface CustomSelectProps {
  value: string | number;
  onChange: (value: string) => void;
  options: SelectOption[];
  placeholder?: string;
  className?: string;
  variant?: 'form' | 'filter' | 'pill' | 'sm';
  disabled?: boolean;
  searchable?: boolean;
}

export const CustomSelect: React.FC<CustomSelectProps> = ({
  value,
  onChange,
  options,
  placeholder = 'Chọn...',
  className = '',
  variant = 'form',
  disabled = false,
  searchable = false,
}) => {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');
  const [coords, setCoords] = useState<{
    top: number;
    left: number;
    width: number;
  }>({ top: 0, left: 0, width: 220 });

  const triggerRef = useRef<HTMLButtonElement | null>(null);
  const menuRef = useRef<HTMLDivElement | null>(null);

  const selectedOption = options.find(
    (o) => String(o.value) === String(value)
  );

  const updatePosition = () => {
    if (!triggerRef.current) return;
    const rect = triggerRef.current.getBoundingClientRect();
    setCoords({
      top: rect.bottom + 5,
      left: rect.left,
      width: Math.max(rect.width, 200),
    });
  };

  const toggleOpen = () => {
    if (disabled) return;
    if (!open) {
      updatePosition();
      setSearch('');
    }
    setOpen((prev) => !prev);
  };

  useEffect(() => {
    if (!open) return;

    const handleClickOutside = (e: MouseEvent) => {
      const target = e.target as Node;
      if (
        triggerRef.current &&
        !triggerRef.current.contains(target) &&
        menuRef.current &&
        !menuRef.current.contains(target)
      ) {
        setOpen(false);
      }
    };

    const handleScrollOrResize = () => {
      updatePosition();
    };

    window.addEventListener('mousedown', handleClickOutside);
    window.addEventListener('scroll', handleScrollOrResize, true);
    window.addEventListener('resize', handleScrollOrResize);
    return () => {
      window.removeEventListener('mousedown', handleClickOutside);
      window.removeEventListener('scroll', handleScrollOrResize, true);
      window.removeEventListener('resize', handleScrollOrResize);
    };
  }, [open]);

  const filteredOptions = options.filter(
    (o) =>
      !search ||
      o.label.toLowerCase().includes(search.toLowerCase()) ||
      (o.subLabel && o.subLabel.toLowerCase().includes(search.toLowerCase()))
  );

  const showSearch = searchable || options.length >= 7;

  return (
    <>
      <button
        ref={triggerRef}
        type="button"
        disabled={disabled}
        className={`custom-select-trigger variant-${variant} ${
          open ? 'is-open' : ''
        } ${className}`}
        onClick={toggleOpen}
      >
        <span className="custom-select-label">
          {selectedOption ? selectedOption.label : placeholder}
        </span>
        <span className={`custom-select-arrow ${open ? 'rotated' : ''}`}>
          ▾
        </span>
      </button>

      {open &&
        createPortal(
          <div
            ref={menuRef}
            className="custom-select-menu"
            style={{
              top: coords.top,
              left: coords.left,
              minWidth: coords.width,
            }}
          >
            {showSearch && (
              <div className="custom-select-search-wrap">
                <input
                  type="text"
                  className="custom-select-search-input"
                  placeholder="🔍 Tìm nhanh..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  autoFocus
                />
              </div>
            )}
            <div className="custom-select-options-list">
              {filteredOptions.map((opt) => {
                const isSelected = String(opt.value) === String(value);
                return (
                  <div
                    key={String(opt.value)}
                    className={`custom-select-option ${
                      isSelected ? 'selected' : ''
                    }`}
                    onClick={() => {
                      onChange(String(opt.value));
                      setOpen(false);
                    }}
                  >
                    <div className="custom-select-option-text">
                      <span>{opt.label}</span>
                      {opt.subLabel && (
                        <small className="custom-select-option-sub">
                          {opt.subLabel}
                        </small>
                      )}
                    </div>
                    {isSelected && (
                      <span className="custom-select-check">✓</span>
                    )}
                  </div>
                );
              })}
              {filteredOptions.length === 0 && (
                <div className="custom-select-empty">
                  Không có kết quả phù hợp
                </div>
              )}
            </div>
          </div>,
          document.body
        )}
    </>
  );
};
