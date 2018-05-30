/*
export const someMutation = (state) => {}
 */

export const updateDrawerState = (state, opened) => {
  state.drawerState = opened
}

// src/store/showcase/state.js

export const updateMobileState = (state, opened) => {
  state.mobileMode = opened
}
