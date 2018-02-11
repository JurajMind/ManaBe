<template>
    <v-card>
        <v-bottom-nav absolute
                      shift
                      :value="true"
                      :active.sync="e2"
                      :color="computedColor">
            <v-btn dark>
                <span>Video</span>
                <v-icon>ondemand_video</v-icon>
            </v-btn>
            <v-btn dark>
                <span>Music</span>
                <v-icon>music_note</v-icon>
            </v-btn>
            <v-btn dark>
                <span>Book</span>
                <v-icon>book</v-icon>
            </v-btn>
            <v-btn dark>
                <span>Image</span>
                <v-icon>image</v-icon>
            </v-btn>
        </v-bottom-nav>
    </v-card>
</template>