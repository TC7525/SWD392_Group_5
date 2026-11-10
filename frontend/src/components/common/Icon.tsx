import React from 'react';

/** Flaticon Uicons — regular rounded (outline), not solid. */
export type IconName =
  | 'add'
  | 'angle-double-small-left'
  | 'angle-double-small-right'
  | 'angle-small-down'
  | 'angle-small-right'
  | 'apps'
  | 'arrow-down'
  | 'arrow-up'
  | 'ban'
  | 'bell'
  | 'book'
  | 'box'
  | 'boxes'
  | 'bookmark'
  | 'building'
  | 'bulb'
  | 'calendar'
  | 'chart-histogram'
  | 'chart-line-up'
  | 'chart-pie'
  | 'check'
  | 'clipboard-list'
  | 'clock'
  | 'circle-small'
  | 'coins'
  | 'cross-small'
  | 'crown'
  | 'disk'
  | 'document'
  | 'exchange'
  | 'file-excel'
  | 'folder'
  | 'handshake'
  | 'home'
  | 'inbox-in'
  | 'inbox-out'
  | 'list'
  | 'marker'
  | 'memo'
  | 'menu-burger'
  | 'money'
  | 'moon'
  | 'pen-nib'
  | 'pencil'
  | 'print'
  | 'refresh'
  | 'scale'
  | 'search'
  | 'settings'
  | 'settings-sliders'
  | 'shield-check'
  | 'sign-in-alt'
  | 'sign-out-alt'
  | 'tags'
  | 'time-past'
  | 'trash'
  | 'triangle-warning'
  | 'trophy'
  | 'truck-side'
  | 'user-add'
  | 'user-gear'
  | 'users'
  | 'warehouse-alt';

interface IconProps {
  name: IconName;
  className?: string;
  color?: string;
}

export const Icon: React.FC<IconProps> = ({ name, className, color }) => (
  <span
    className={`fi-rr-${name} app-icon${className ? ` ${className}` : ''}`}
    style={color ? { color } : undefined}
    aria-hidden="true"
  />
);
